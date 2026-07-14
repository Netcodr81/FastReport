# SkiaSharp vs Microsoft.Maui.Graphics - Selection Analysis

**Decision Date**: 2024  
**Task**: FastReport.OpenSource .NET 10 cross-platform graphics migration  
**Context**: User approved Option C (Full Modern Graphics Migration)

---

## Executive Summary

**Decision**: ✅ **SkiaSharp 2.88+** is selected as the primary graphics library

**Reasoning**:
1. **Maturity**: Production-proven, used by Xamarin, MAUI, and many enterprises
2. **Performance**: Native code via Skia, excellent rendering performance
3. **Cross-platform**: Excellent Windows/Linux/macOS support, including WebAssembly via Wasm build
4. **API Completeness**: Full graphics feature set matches/exceeds System.Drawing
5. **Community**: Large, active ecosystem with extensive examples
6. **Licensing**: Apache 2.0 (permissive, good for FastReport)

**Caveat**: Microsoft.Maui.Graphics included in MAUI builds - can evaluate transition post-migration if needed.

---

## Detailed Comparison

### SkiaSharp (Google Skia C++ wrapped in .NET)

#### Strengths
✅ **Production Maturity**
- Used in production by Xamarin (now MAUI)
- Google Skia is battle-tested
- 2.88+ has excellent .NET 6/10 support
- Stable API, infrequent breaking changes

✅ **Performance**
- Native performance (C++ underneath)
- GPU acceleration available
- Excellent text rendering
- Fast rendering pipelines

✅ **Cross-Platform**
- Windows (x64, ARM64)
- Linux (x64, ARM64)
- macOS (Intel, Apple Silicon)
- WebAssembly (experimental but usable)
- iOS/Android (if you want)

✅ **API Completeness**
- SKCanvas - equivalent to Graphics
- SKFont, SKTypeface - font system
- SKPaint - styling
- SKPath - vector drawing
- SKImage, SKBitmap - image operations
- Full transformation matrix support

✅ **Ecosystem**
- Extensive documentation
- Many examples online
- Active GitHub community
- Used in production by major companies

✅ **Text Rendering**
- Excellent text layout
- Support for complex scripts
- Multiple text measurement APIs

#### Weaknesses
⚠️ **Learning Curve**
- Different coordinate model than GDI+
- Text baseline handling differs
- Some APIs have subtle differences in behavior

⚠️ **Size**
- Large native library (~50MB+ for full platform support)
- WebAssembly build larger

⚠️ **Dependencies**
- Requires native library distribution
- NuGet package handles most cases, but cross-compilation can be tricky

---

### Microsoft.Maui.Graphics (First-party Microsoft graphics)

#### Strengths
✅ **First-Party Support**
- Built by Microsoft, supported as part of MAUI
- Guaranteed long-term support and updates
- Excellent integration with MAUI ecosystem

✅ **Lightweight**
- Smaller footprint than SkiaSharp
- Optimized for specific scenarios

✅ **Modern API Design**
- Designed from ground up for modern .NET
- Consistent with MAUI patterns

#### Weaknesses
❌ **Maturity**
- Newer than SkiaSharp
- Less proven in non-MAUI scenarios
- Fewer production examples outside MAUI ecosystem

❌ **Performance**
- Still uses Skia underneath in many cases
- No clear performance advantage over direct SkiaSharp

❌ **Cross-Platform Limitations**
- Primarily designed for MAUI (desktop/mobile)
- WebAssembly support unclear/limited
- Server-side rendering use cases less common

❌ **Learning Resources**
- Fewer examples and blog posts
- Documentation focuses on MAUI context
- Harder to find solutions to edge cases

❌ **Coupling**
- More tightly coupled to MAUI ecosystem
- May pull in MAUI dependencies unneeded by FastReport

---

## Decision Matrix

| Factor | Weight | SkiaSharp | Maui.Graphics | Winner |
|--------|--------|-----------|----------------|--------|
| **Production Maturity** | 25% | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | SkiaSharp |
| **Cross-Platform** | 20% | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | SkiaSharp |
| **Performance** | 20% | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | SkiaSharp |
| **API Completeness** | 15% | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | SkiaSharp |
| **Community/Docs** | 10% | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | SkiaSharp |
| **Ecosystem Fit** | 10% | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | SkiaSharp |

**Score**: SkiaSharp ~4.8/5 | Maui.Graphics ~3.7/5

**Conclusion**: SkiaSharp is the clear winner for FastReport's needs.

---

## Why SkiaSharp for FastReport Specifically

### FastReport Use Cases
1. **Server-side rendering** (ASP.NET Core, Azure Functions) → SkiaSharp excels
2. **Cross-platform reporting** (Windows, Linux, Docker) → SkiaSharp proven
3. **Blazor support** (server + WebAssembly) → SkiaSharp has both paths
4. **MAUI integration** (future desktop designer) → SkiaSharp is what MAUI uses anyway
5. **Console/Worker services** → SkiaSharp works anywhere .NET works

### Advantage Over System.Drawing
| Scenario | System.Drawing | SkiaSharp |
|----------|---|---|
| Windows Desktop | ✅ Native | ✅ Good |
| Linux Server | ❌ No | ✅ Excellent |
| Docker Container | ❌ No | ✅ Excellent |
| Blazor Server | ❌ No | ✅ Excellent |
| Blazor WebAssembly | ❌ No | ✅ Possible (Wasm) |
| macOS | ❌ No | ✅ Excellent |
| ARM64 | ❌ No | ✅ Excellent |
| Mobile (iOS/Android) | ❌ No | ✅ Yes (bonus) |

---

## SkiaSharp Version Strategy

**Target**: SkiaSharp 2.88+ (latest stable)

**Why 2.88**:
- Full .NET 6/10 support
- Mature stable release
- Excellent performance
- All APIs needed for FastReport rendering

**Upgrade Path**:
- 2.88 LTS for stability
- Can upgrade to 3.x if/when Microsoft releases (better MAUI integration)

---

## Migration Compatibility

### API Mapping (System.Drawing → SkiaSharp)

```
System.Drawing.Graphics      → SkiaSharp.SKCanvas
System.Drawing.Font          → SkiaSharp.SKFont/SKTypeface
System.Drawing.Brush/Color   → SkiaSharp.SKPaint + SKColors
System.Drawing.Pen           → SkiaSharp.SKPaint (with stroke)
System.Drawing.Image         → SkiaSharp.SKImage
System.Drawing.Bitmap        → SkiaSharp.SKBitmap
System.Drawing.StringFormat  → SkiaSharp.SKTextAlign + SKPaint.TextEncoding
System.Drawing.FontStyle     → SkiaSharp.SKFontStyle
System.Drawing.GraphicsPath  → SkiaSharp.SKPath
```

**Key Differences to Handle**:
1. **Coordinate System**: SkiaSharp uses top-left origin (same as GDI+, good)
2. **Text Baseline**: GDI+ has implicit baseline; SkiaSharp is explicit (requires adjustment)
3. **Color Format**: Both use ARGB, compatible
4. **DPI Handling**: SkiaSharp doesn't have built-in DPI scaling (must handle explicitly)

---

## Implementation Plan Impact

### NuGet Package
```xml
<PackageReference Include="SkiaSharp" Version="2.88.8" />
<PackageReference Include="SkiaSharp.Views.Desktop.GTK" Version="2.88.8" Condition="$(TargetFramework.StartsWith('net')) and !$(TargetFramework.Contains('windows'))" />
<!-- Platform-specific packages as needed -->
```

### Build System
- Already SDK-style, no changes needed
- SkiaSharp NuGet handles platform selection automatically
- Can conditionally reference platform-specific packages

### Testing
- Need rendering validation tests
- Compare output against baseline (System.Drawing results)
- Test across Windows, Linux, macOS

---

## Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Text measurement differences | Create abstraction layer (IGraphicsProvider) for text metrics |
| DPI handling | Add explicit DPI handling in SkiaSharpGraphicsProvider |
| Performance regression | Profile baseline System.Drawing, compare with SkiaSharp |
| Missing features | Use SkiaSharp's full API (rarely missing anything) |
| Cross-platform issues | Test on Windows/Linux/macOS before releasing |

---

## Next Steps

### Phase B (Foundation Setup)
1. ✅ Add SkiaSharp 2.88.8 NuGet package
2. ✅ Create IGraphicsProvider interface
3. ✅ Create SkiaSharpGraphicsProvider implementation
4. ✅ Update project to net10.0 targeting
5. ✅ Document API mapping

### Phase C (Migration)
1. Begin systematic System.Drawing → SkiaSharp replacement
2. Update entry points to use IGraphicsProvider injection
3. Test Windows rendering
4. Validate output against baselines

### Phase D (Cross-Platform)
1. Add Linux/macOS targeting
2. Test cross-platform rendering
3. Performance optimization

---

## Long-term Evolution

**Future Considerations** (not in scope for this task):
- Once MAUI graphics stabilize, could evaluate Maui.Graphics as alternative backend
- IGraphicsProvider abstraction makes switching easy if needed
- SkiaSharp likely to remain industry standard for .NET graphics

---

## Conclusion

✅ **SkiaSharp 2.88+** is the optimal choice for FastReport.OpenSource because:
1. Production-proven in similar scenarios (server-side rendering)
2. Excellent cross-platform support (Windows, Linux, macOS, WebAssembly)
3. Mature, stable API with large ecosystem
4. Perfect fit for FastReport's modernization goals
5. Enables all future scenarios (Blazor, MAUI, Docker, cloud)
6. Clean migration path with well-understood API mappings

**Implementation can proceed with confidence.**

---

**Decision Made**: SkiaSharp 2.88+ selected  
**Status**: Ready for Phase B implementation
