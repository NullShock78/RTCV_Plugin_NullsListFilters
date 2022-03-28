# Null's List Filters

A collection of plugin list filters for the Real Time Corruptor<br>
Current list filters:
<li>Float Tilt Filter

## Float Tilt Filter

To create a Float Tilt Filter, set the first line of the file to this: `@FloatTiltListFilter`<br>

**Format**:<br>
`(PREFIX)[MIN]->[MAX]` Ex: `(ADD)-1.0->2.0`<br>
`(PREFIX)[EXACT]` Ex: `(MUL)3.0`<br>

### Limiter List Features:
Converts bytes into IEEE 754 32-bit floats, checks if the converted value is valid, non-infinite, and between a specified range of values.<br>
Limiter List entries are always prefixed with `(LIM)`, and will be ignored when the list is used as a value list.<br>
All entries with prefixes besides `(LIM)` will be ignored when using the list as a limiter list.<br>

#### Example Limiter List:

When used as a limiter list will match values between 0.5f and 1.0f, and values exactly equal to 2.0f<br>
```
@FloatTiltListFilter
(LIM)0.5->1.0
(LIM)2.0
```

Notes:<br>
Limiter entries containing 0.0f (0x00000000) are not recommended due to the very high number of false positives.<br>
Limiter entries covering large ranges will result in more false positives.<br>

### Value List Features:<br>
Converts bytes into IEEE 754 32-bit floats, performs floating point operations on the data, then converts the result back into an array of bytes.<br>
Adding `#REPEAT:3` will cause the list to apply additional operations to the resulting value, for the specified number of times. (Multiple operations will be performed).<br>


#### Example Value Lists:<br>
**Example 1**: The following list will either divide by 3, multiply by 2, add a random value between -1.0f and 1.0f (inclusive), or subtract a random value between 0.001f and 0.1f
```
@FloatTiltListFilter
(DIV)3.0
(MUL)2.0
(ADD)-1.0->1.0
(SUB)0.001->0.1
```

**Example 2**: repeat 4 additional random ops<br>
```
@FloatTiltListFilter
#REPEAT:4
(DIV)3.0
(MUL)2.0
```

Input: `2f` (`0x40000000` or `0x00000040` flipped)<br>
Example Result: `(((((2f * 2) * 2) / 3) / 3) * 2)` => `1.7777777777777777777f` (`0x3FE38E39` or `0x398EE33F` flipped)<br>

### List Entry Prefixes:
Key:<br>
`X` = A random number chosen from the specified range (or exact)<br>
`value` = output<br>

`LIM`	Specifies a limiter entry, is ignored for value lists<br>
  
`ADD`&ensp;&ensp;Adds `X` to the `value`.<br>
`SUB`&ensp;&ensp;Subtracts `X` from the `value`.<br>
`MUL`&ensp;&ensp;Multiplies the `value` by `X`.<br>
`DIV`&ensp;&ensp;Divides the `value` by `X` (if `X` is zero, will return original `value`).<br>
`SET`&ensp;&ensp;Sets the `value` to `X` directly.<br>
`ABS`&ensp;&ensp;Sets the `value` to its Absolute Value. `X` is ignored.<br>
`NEG`&ensp;&ensp;Sets the `value` to -1.0f times its Absolute Value. `X` is ignored.<br>
`SQRT`&ensp;Sets the `value` to its square root. `X` is ignored.<br>
`RND`&ensp;&ensp;Rounds the `value` up to the nearest integral. Math.Ceiling(`value`). `X` is ignored.<br>
`SIN`&ensp;&ensp;Sets the `value` to its Sine. Math.Sin(`value`). `X` is ignored.<br>
`SIN2`&ensp;Sets the `value` to Math.Sin(`value`) + Math.Sin(`X`).<br>
`COS`&ensp;&ensp;Sets the `value` to its Cosine. Math.Cos(`value`). `X` is ignored.<br>
`COS2`&ensp;Sets the `value` to Math.Cos(`value`) + Math.Cos(`X`).<br>
`TAN`&ensp;&ensp;Sets the `value` to its Tangent. Math.Tan(`value`). `X` is ignored.<br>
`TAN2`&ensp;Sets the `value` to Math.Tan(`value`) + Math.Tan(`X`).<br>
`LOG`&ensp;&ensp;Sets the `value` to its Log. Math.Log(`value`). If `value` <= 0, the original `value` is returned. `X` is ignored.<br>
`LOG2`&ensp;Sets the `value` to Math.Log(`value`) + Math.Log(`X`). If `value` <= 0 or `X` <= 0, the original `value` is returned.<br>
`POW`&ensp;&ensp;Raises `value` to the `X`th power. Math.Pow(`value`, `X`).<br>

### Notes:<br>
List Precision is locked at 4 bytes/32 bits<br>
Infinity and NAN values are currently not supported for limiter lists<br>
