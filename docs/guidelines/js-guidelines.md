# JS guidelines

## Contents

- [Guard every exported function's parameters at runtime](#guard-every-exported-functions-parameters-at-runtime)

## Guard every exported function's parameters at runtime

An exported function is external API: anything can call it, including code TypeScript never
type-checked against this module. A parameter's type signature is not enforced past the boundary —
nothing at runtime rejects a caller passing the wrong shape.

Check every received value before using it — `instanceof`, `typeof`, or an equivalent runtime guard
— rather than trusting the declared parameter type.
