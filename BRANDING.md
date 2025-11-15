# InstallVibe Branding Package

## App Identity

**Name:** InstallVibe
**Tagline:** "Guided Installations. Zero Guesswork."

**Brand Statement:**
InstallVibe empowers installation technicians with clear, step-by-step guides for complex equipment installations. By combining visual aids, safety notes, and progress tracking, we eliminate uncertainty and ensure every installation is completed correctly the first time. Built for professionals who demand precision and efficiency.

**UX Tone:**
- **Concise**: Every word counts. No fluff, just actionable information.
- **Technician-Friendly**: Use industry terminology, respect expertise.
- **Confidence-Building**: Clear instructions reduce anxiety and errors.
- **Professional**: Serious about safety and accuracy.

---

## Color Palette

### Primary Brand Colors

| Color Name | HEX | RGB | Usage |
|------------|-----|-----|-------|
| Brand Primary | `#0078D4` | `rgb(0, 120, 212)` | Primary actions, links, focus states |
| Brand Primary Dark | `#005A9E` | `rgb(0, 90, 158)` | Hover states, pressed buttons |
| Brand Primary Light | `#4A9EE0` | `rgb(74, 158, 224)` | Highlights, secondary accents |
| Brand Accent | `#FF6B35` | `rgb(255, 107, 53)` | Call-to-action, important highlights |
| Brand Success | `#0F9D58` | `rgb(15, 157, 88)` | Completed steps, success messages |
| Brand Warning | `#F9AB00` | `rgb(249, 171, 0)` | Warnings, caution notices |
| Brand Error | `#D32F2F` | `rgb(211, 47, 47)` | Errors, critical safety notes |

### Neutral Colors

| Color Name | HEX | RGB | Usage |
|------------|-----|-----|-------|
| Neutral Gray 50 | `#FAFAFA` | `rgb(250, 250, 250)` | Lightest backgrounds |
| Neutral Gray 100 | `#F5F5F5` | `rgb(245, 245, 245)` | Card backgrounds (light mode) |
| Neutral Gray 200 | `#EEEEEE` | `rgb(238, 238, 238)` | Borders, dividers |
| Neutral Gray 300 | `#E0E0E0` | `rgb(224, 224, 224)` | Disabled states |
| Neutral Gray 500 | `#9E9E9E` | `rgb(158, 158, 158)` | Secondary text |
| Neutral Gray 700 | `#616161` | `rgb(97, 97, 97)` | Body text (light mode) |
| Neutral Gray 900 | `#212121` | `rgb(33, 33, 33)` | Headings, primary text |

---

## Typography

### Font Stack
Primary: **Segoe UI** (System default for WinUI 3)
Fallback: **Segoe UI Variable**, **Arial**, sans-serif

### Type Scale

| Style | Size | Weight | Line Height | Usage |
|-------|------|--------|-------------|-------|
| Display | 32px | Bold | 40px | Hero sections, major headings |
| Heading | 24px | SemiBold | 32px | Page titles, section headers |
| Title | 20px | SemiBold | 28px | Card titles, subsection headers |
| Subtitle | 16px | Normal | 24px | Secondary headings, emphasis |
| Body | 14px | Normal | 20px | Body text, descriptions |
| Caption | 12px | Normal | 16px | Metadata, timestamps, helper text |

---

## Icon Assets

### Required App Icons

All icons should be generated from a single master SVG with the following specifications:

**Icon Geometry:**
- **Shape**: Rounded square with 12px corner radius
- **Foreground**: Stylized wrench + checklist combination
  - Wrench: Positioned at 45° angle, takes up 60% of space
  - Checklist: Small checkmarks visible behind/beside wrench
- **Colors**:
  - Background: Brand Primary (`#0078D4`)
  - Foreground: White (`#FFFFFF`)
  - Accent: Brand Accent (`#FF6B35`) for check marks

**Required Sizes and File Names:**

| File Name | Size | Purpose |
|-----------|------|---------|
| `Square44x44Logo.png` | 44×44px | App list, small tile |
| `Square150x150Logo.png` | 150×150px | Medium tile, start menu |
| `Square71x71Logo.png` | 71×71px | Small tile |
| `Square310x310Logo.png` | 310×310px | Large tile |
| `Wide310x150Logo.png` | 310×150px | Wide tile |
| `SplashScreen.png` | 620×300px | App splash screen |
| `StoreLogo.png` | 50×50px | Store listing |
| `BadgeLogo.png` | 24×24px | Lock screen badge |

**Folder Placement:**
```
InstallVibe/
└── Assets/
    ├── Square44x44Logo.png
    ├── Square44x44Logo.scale-125.png
    ├── Square44x44Logo.scale-150.png
    ├── Square44x44Logo.scale-200.png
    ├── Square150x150Logo.png
    ├── Square150x150Logo.scale-125.png
    ├── Square150x150Logo.scale-150.png
    ├── Square150x150Logo.scale-200.png
    ├── Square71x71Logo.png
    ├── Square310x310Logo.png
    ├── Wide310x150Logo.png
    ├── SplashScreen.png
    ├── SplashScreen.scale-125.png
    ├── SplashScreen.scale-150.png
    ├── SplashScreen.scale-200.png
    ├── StoreLogo.png
    └── BadgeLogo.png
```

---

## Design Tokens

### Corner Radius
- **Small:** 4px — Buttons, badges, small controls
- **Medium:** 8px — Cards, inputs, general containers
- **Large:** 12px — Large cards, modals
- **X-Large:** 16px — Hero cards, special containers

### Shadows
- **Elevation 1:** 2dp — Subtle lift for cards
- **Elevation 2:** 4dp — Buttons, interactive elements
- **Elevation 3:** 8dp — Dropdowns, tooltips
- **Elevation 4:** 16dp — Modals, dialogs

### Spacing Scale
- **XS:** 4px
- **S:** 8px
- **M:** 16px
- **L:** 24px
- **XL:** 32px
- **XXL:** 48px

---

## Accessibility

### Color Contrast Ratios
- **Minimum (WCAG AA):**
  - Normal text: 4.5:1
  - Large text (18px+): 3:1
- **Enhanced (WCAG AAA):**
  - Normal text: 7:1
  - Large text: 4.5:1

**All InstallVibe UI meets WCAG AA standards minimum.**

### Touch Targets
- Minimum size: **44×44 pixels** for all interactive elements
- Spacing: Minimum **8px** between adjacent touch targets

---

## Voice and Messaging Examples

### Success Messages
✅ "Step completed successfully!"
✅ "Guide saved. You're all set."
✅ "Installation progress: 75% complete."

### Error Messages
❌ "Connection failed. Please check your network."
❌ "Unable to save. Try again."
❌ "Invalid input. Please enter a valid value."

### Safety Warnings
⚠️ "DANGER: Ensure power is off before proceeding."
⚠️ "CAUTION: Wear protective eyewear."
⚠️ "WARNING: High voltage area."

---

## File Generation Tools

### Recommended Icon Generation
1. **Figma** - Design master icon at 512×512px
2. **Export** as SVG
3. **Rasterize** using:
   - [ImageMagick](https://imagemagick.org/)
   - [GIMP](https://www.gimp.org/)
   - [Photoshop](https://www.adobe.com/products/photoshop.html)

### Example ImageMagick Commands
```bash
# Generate 44x44 logo
convert master-icon.svg -resize 44x44 Square44x44Logo.png

# Generate 150x150 logo
convert master-icon.svg -resize 150x150 Square150x150Logo.png

# Generate splash screen
convert master-icon.svg -resize 620x300 -background "#0078D4" -gravity center -extent 620x300 SplashScreen.png
```

---

## Implementation Checklist

- [x] Colors.xaml created with full palette
- [x] Styles.xaml created with typography and card styles
- [x] ControlStyles.xaml created with button, input, and control overrides
- [x] App.xaml updated to merge resource dictionaries
- [ ] Icon assets generated and placed in Assets/ folder
- [ ] Package.appxmanifest updated with icon references
- [x] Branding documentation complete

---

**Last Updated:** 2025-01-15
**Version:** 1.0.0
