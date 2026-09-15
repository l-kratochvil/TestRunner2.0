namespace TestRunner.WebApp.Components;

using System.ComponentModel;

using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

/// <summary>
/// A component that redraws itself whenever the view model it is drawn from reports a change.
/// </summary>
/// <remarks>
/// A component that overrides <see cref="CreateViewModel"/> owns what it makes and disposes of it;
/// one that does not is drawn from the view model a <see cref="Primitives.DataContext{TViewModel}"/>
/// above it cascades, and leaves the disposing to whoever put it there. Either way the view model is
/// listened to for as long as the component lives.
/// </remarks>
/// <typeparam name="TViewModel">The view model the component is drawn from.</typeparam>
public abstract class MvvmComponentBase<TViewModel> : FluxorComponent
    where TViewModel : class, INotifyPropertyChanged
{
    // Tidying up reaches the view model through the field and not through the property, so that
    // disposal does not make the very thing it is about to throw away.
    private TViewModel? viewModel;

    private bool ownsViewModel;

    private int renderPending;

    private volatile bool disposed;

    /// <summary>
    /// Gets the view model this component is drawn from, found on first use.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The component neither makes a view model nor stands under a
    /// <see cref="Primitives.DataContext{TViewModel}"/> that cascades one.
    /// </exception>
    protected TViewModel ViewModel
        => this.viewModel ??= this.ResolveViewModel();

    /// <summary>
    /// Gets or sets the view model cascaded to this component, if there is one.
    /// </summary>
    /// <remarks>
    /// Read through <see cref="ViewModel"/> rather than directly, so that a component drawn from a
    /// cascaded view model and one drawn from a view model of its own are written the same way.
    /// </remarks>
    [CascadingParameter]
    private TViewModel? DataContext { get; set; }

    /// <summary>
    /// Creates the view model this component is drawn from.
    /// </summary>
    /// <returns>
    /// The view model, which this component owns from then on, or <see langword="null"/> to be
    /// drawn from the one cascaded to it.
    /// </returns>
    protected virtual TViewModel? CreateViewModel()
        => null;

    /// <summary>
    /// Says whether a change of the named property is one this component is drawn from.
    /// </summary>
    /// <remarks>
    /// Answered on the thread that raised the change, which need not be this component's circuit,
    /// so the answer may be read from the view model but nothing may be drawn from here.
    /// </remarks>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the component should re-render; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldRerenderOn(string? propertyName)
        => true;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
        base.OnInitialized();
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        // The renderer disposes of a component that is IAsyncDisposable — which FluxorComponent is
        // — through that alone, so everything this class owns is let go of here. An IDisposable of
        // its own would never be called.
        if (disposing)
        {
            this.disposed = true;

            if (this.viewModel is not null)
            {
                this.viewModel.PropertyChanged -= this.OnViewModelPropertyChanged;

                // A cascaded view model outlives the components drawn from it, so only the one this
                // component made is thrown away here.
                if (this.ownsViewModel)
                {
                    switch (this.viewModel)
                    {
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync();
                            break;

                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                    }
                }
            }
        }

        await base.DisposeAsyncCore(disposing);
    }

    private TViewModel ResolveViewModel()
    {
        // Making a view model is what claims it, so a component that makes none is drawn from the
        // cascaded one without ever being in a position to throw it away.
        if (this.CreateViewModel() is { } created)
        {
            this.ownsViewModel = true;

            return created;
        }

        // Standing outside a data context leaves the component with nothing to draw, which shows up
        // as a blank where the control should be rather than as a mistake, so it is said out loud.
        return this.DataContext ?? throw new InvalidOperationException(
            $"{this.GetType().Name} neither creates a view model nor is placed inside a " +
            $"{nameof(Primitives.DataContext<TViewModel>)} of {typeof(TViewModel).Name}.");
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (this.disposed || !this.ShouldRerenderOn(e.PropertyName))
        {
            return;
        }

        // A view model fed by a store is told of a change on the thread of whoever made it, which
        // is somebody else's circuit as often as not, so several threads can arrive here at once.
        // Exchanging the flag rather than reading it and then setting it is what makes one render
        // out of a burst a guarantee instead of a likelihood.
        if (Interlocked.Exchange(ref this.renderPending, 1) is 1)
        {
            return;
        }

        try
        {
            _ = this.InvokeAsync(() =>
            {
                // Cleared before drawing, so that a change raised while the component renders asks
                // for the next render instead of being swallowed by the one under way.
                Interlocked.Exchange(ref this.renderPending, 0);

                if (!this.disposed)
                {
                    this.StateHasChanged();
                }
            });
        }
        catch (ObjectDisposedException)
        {
            // The circuit went away between the change and this call. Letting the exception out
            // would stop the change reaching the listeners after this one.
            Interlocked.Exchange(ref this.renderPending, 0);
        }
    }
}