# Keyframes
Simple tweening for any data, akin to AnimationCurve &amp; Gradient

[![twitter](https://img.shields.io/twitter/follow/_neonage?style=social)](https://twitter.com/_neonage)\
[![discord online](https://img.shields.io/discord/830405926078644254?label=Open%20Labs&logo=discord&style=social)](https://discord.gg/uF3sJFMA2j)

https://user-images.githubusercontent.com/29812914/168467641-3d0c5140-c0b7-41d6-885e-c0acfeec7ad2.mp4

### Usage
```csharp
// See UITween.cs for example
public Keyframes<Vector2> sizeFrames;

sizeFrames.Evaluate(lerp, out var size1, out var size2, out var sizeTime);

if (size1 != default || size2 != default)
  rect.sizeDelta = Vector2.Lerp(size1, size2, sizeTime);
```

## Open Labs Community
[![join discord](https://user-images.githubusercontent.com/29812914/121816656-0cb93080-cca7-11eb-954a-344cfd31f530.png)](https://discord.gg/uF3sJFMA2j)
