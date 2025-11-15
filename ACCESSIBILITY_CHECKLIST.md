# InstallVibe Accessibility Testing Checklist

This checklist ensures InstallVibe meets WCAG 2.1 Level AA accessibility standards and provides an excellent experience for all users, including those using assistive technologies.

## 📋 Manual Testing Checklist

### Keyboard Navigation

- [ ] **Tab Navigation**: All interactive elements are reachable via Tab key
- [ ] **Focus Indicators**: Visible focus outline appears on all interactive elements (3px outline)
- [ ] **Tab Order**: Logical tab order follows visual layout (left-to-right, top-to-bottom)
- [ ] **Escape Key**: Closes dialogs and cancels operations
- [ ] **Enter Key**: Activates buttons and submits forms
- [ ] **Arrow Keys**: Navigate through lists and step sequences
- [ ] **Keyboard Shortcuts Work**:
  - Alt+Left Arrow: Navigate back
  - Alt+Home: Go to Guide Library
  - Ctrl+Comma: Open Settings
  - F1: Show help (when implemented)
  - Left/Right Arrow: Previous/Next step in viewer
  - Space: Toggle step completion

### Screen Reader Compatibility

- [ ] **Login Screen**: All controls have accessible names
  - Username field labeled "Username"
  - Password field labeled "Password"
  - Login button announces "Login"
- [ ] **Guide Library**: Guides announced with title and category
- [ ] **Step Viewer**: Each step announces step number, title, and status
- [ ] **Admin Dashboard**: Create/Edit/Delete buttons have clear names
- [ ] **Settings**: Theme toggle announces current selection
- [ ] **Error Messages**: Announced automatically (LiveSetting="Assertive")
- [ ] **Success Messages**: Announced politely (LiveSetting="Polite")
- [ ] **Dialogs**: Title and content read correctly
- [ ] **Lists**: Item count and position announced ("Item 1 of 5")

### Color and Contrast

- [ ] **Minimum Contrast Ratios Met**:
  - Normal text (14px): 4.5:1 contrast ratio
  - Large text (18px+ or 14px bold): 3:1 contrast ratio
  - UI components: 3:1 contrast ratio
- [ ] **Color Not Sole Indicator**: Status uses icons + color
  - Completed: Green background + checkmark icon
  - Pending: Gray background + circle icon
  - Error: Red background + X icon + error text
- [ ] **High Contrast Mode**: App remains usable in Windows High Contrast
  - Text readable
  - Borders visible
  - Focus indicators prominent

### Text Scaling

- [ ] **Font Size Setting Works**: Small/Normal/Large scaling applies immediately
- [ ] **200% Zoom**: Content remains readable at 200% browser zoom
- [ ] **No Horizontal Scrolling**: At 200% zoom, no horizontal scrolling required
- [ ] **Text Reflow**: Long labels wrap properly, don't truncate

### Touch and Motor Accessibility

- [ ] **Minimum Touch Targets**: All interactive elements ≥ 44x44 pixels
- [ ] **Adequate Spacing**: At least 8px spacing between touch targets
- [ ] **Large Action Buttons**: Primary actions ≥ 88px wide
- [ ] **No Hover-Only Actions**: All functions accessible without hover
- [ ] **Double-Tap Protection**: Confirm destructive actions (delete, clear data)

### Visual Clarity

- [ ] **Clear Headings**: Page titles use heading styles with AutomationProperties.HeadingLevel
- [ ] **Logical Information Hierarchy**: Important info visually prominent
- [ ] **Sufficient Line Height**: Body text has 1.5x line height
- [ ] **Readable Font Sizes**: Minimum 14px for body text, 12px for captions
- [ ] **No Content Loss**: No information hidden behind overlays or off-screen

### Forms and Inputs

- [ ] **Label Association**: All inputs have associated labels
- [ ] **Placeholder Not Sole Label**: Labels remain visible when input has focus
- [ ] **Error Identification**: Errors clearly indicated with icon, color, and text
- [ ] **Error Suggestions**: Helpful error messages explain how to fix
- [ ] **Required Fields Marked**: Asterisk or "(required)" text
- [ ] **Input Purpose Clear**: Field purpose obvious from label/context

### Media and Content

- [ ] **Images Have Alt Text**: All informative images have meaningful alt text
- [ ] **Decorative Images Hidden**: Decorative images have empty alt="" or AutomationProperties.AccessibilityView="Raw"
- [ ] **Icons With Labels**: Icons paired with text labels, not icons alone
- [ ] **Video Captions**: Tutorial videos have captions (when implemented)

### Timing and Animations

- [ ] **No Auto-Advance**: Step viewer doesn't auto-advance
- [ ] **Animations Can Be Reduced**: Respect Windows "Reduce motion" setting (future enhancement)
- [ ] **No Time Limits**: No operations have time limits
- [ ] **Loading Indicators**: Progress shown for long operations

## 🔬 Automated Testing

### Screen Reader Testing

Run with **Narrator** (Windows) or **NVDA** (free download):

```
1. Launch Narrator: Win+Ctrl+Enter
2. Navigate app with:
   - Tab: Next element
   - Shift+Tab: Previous element
   - Caps+Arrow: Read by word/line
   - Caps+Ctrl+R: Start continuous reading
3. Verify announcements match expectations
```

### Keyboard Testing

```
1. Disconnect mouse
2. Complete full user journey:
   - Login
   - Browse guides
   - View step-by-step instructions
   - Access Settings
   - Logout
3. Ensure all functions accessible via keyboard
```

### Contrast Testing

Tools:
- **Windows Accessibility Insights**: Free automated testing tool
- **Manual**: Use browser DevTools > Accessibility panel
- **Online**: WebAIM Contrast Checker (https://webaim.org/resources/contrastchecker/)

Verify:
- Primary button (white text on #0078D4): Must be ≥ 4.5:1
- Body text (#212121 on #FFFFFF): Must be ≥ 4.5:1
- Secondary text (#757575 on #FFFFFF): Must be ≥ 4.5:1

### High Contrast Testing

```
1. Enable Windows High Contrast: Alt+Left Shift+Print Screen
2. Navigate entire app
3. Verify all content visible and usable
4. Check focus indicators prominent
```

## 📊 Automated Test Examples

### Example UI Test (Pseudo-code)

```csharp
[TestMethod]
public void LoginButton_HasAccessibleName()
{
    var loginButton = FindElement(By.Name("LoginButton"));
    var accessibleName = loginButton.GetAutomationProperty("Name");

    Assert.AreEqual("Login", accessibleName);
}

[TestMethod]
public void StepList_KeyboardNavigable()
{
    var stepList = FindElement(By.AutomationId("StepsRepeater"));
    stepList.SetFocus();

    // Press Down arrow
    SendKeys.SendWait("{DOWN}");

    var focusedItem = GetFocusedElement();
    Assert.IsTrue(focusedItem.GetType().Name.Contains("Step"));
}

[TestMethod]
public void ThemeToggle_PersistsAcrossRestart()
{
    // Set theme to Dark
    var settingsPage = NavigateToSettings();
    var themeComboBox = settingsPage.FindElement(By.AutomationId("ThemeComboBox"));
    themeComboBox.SelectItem("Dark");

    // Restart app
    RestartApplication();

    // Verify theme persisted
    var settings = LoadSettings();
    Assert.AreEqual(ElementTheme.Dark, settings.Theme);
}

[TestMethod]
public void StepCompletion_AnnouncedToScreenReader()
{
    var step = FindElement(By.AutomationId("Step1"));
    var completionButton = step.FindElement(By.AutomationId("MarkCompleteButton"));

    // Click to complete
    completionButton.Click();

    // Verify live region updated
    var statusRegion = FindElement(By.AutomationId("StepStatusLiveRegion"));
    var liveUpdate = statusRegion.GetAutomationProperty("LiveSetting");

    Assert.AreEqual("Polite", liveUpdate);
    Assert.IsTrue(statusRegion.Text.Contains("Completed"));
}
```

## ✅ Acceptance Criteria

All items in this checklist must pass before releasing a new version.

### Critical (Blocking Release)

- All keyboard navigation tests pass
- Screen reader announces critical information correctly
- Minimum contrast ratios met for all text
- Touch targets meet minimum 44x44 size
- High contrast mode functional

### High Priority (Fix Before Next Release)

- All WCAG 2.1 Level AA criteria met
- Automated accessibility tests passing
- No critical accessibility issues in user testing

### Nice to Have (Future Enhancement)

- WCAG 2.1 Level AAA compliance
- Reduced motion support
- Voice control compatibility
- Customizable keyboard shortcuts

## 📖 Resources

- **WCAG 2.1**: https://www.w3.org/WAI/WCAG21/quickref/
- **Windows Accessibility Insights**: https://accessibilityinsights.io/
- **WebAIM**: https://webaim.org/
- **Microsoft Accessibility Guidelines**: https://docs.microsoft.com/en-us/windows/apps/design/accessibility/accessibility-overview
- **Color Contrast Analyzer**: https://www.tpgi.com/color-contrast-checker/

## 👥 User Testing

Include users with disabilities in testing:
- Screen reader users
- Keyboard-only users
- Users with low vision
- Users with color blindness
- Users with motor impairments

Recruit testers through:
- Local disability advocacy groups
- Accessibility testing services
- User research panels

---

**Last Updated**: 2025-11-15
**Version**: 1.0
**Owner**: InstallVibe Development Team
