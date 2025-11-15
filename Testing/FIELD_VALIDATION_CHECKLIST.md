# InstallVibe Field Validation Checklist

**Pre-Release Field Testing**
**Version:** _____________
**Tester:** _____________
**Date:** _____________
**Location:** _____________
**Device:** _____________

---

## Instructions

1. Complete each section in order
2. Mark each item: ✓ (Pass), ✗ (Fail), N/A (Not Applicable)
3. Record issues in the Notes column
4. Submit completed checklist to QA team

---

## Section 1: Installation & Setup (5 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 1.1 | MSIX package installs without errors | ☐ | ☐ | ☐ | |
| 1.2 | Certificate is trusted (no security warnings) | ☐ | ☐ | ☐ | |
| 1.3 | App appears in Start Menu | ☐ | ☐ | ☐ | |
| 1.4 | App launches on first run (< 5 seconds) | ☐ | ☐ | ☐ | |
| 1.5 | No error dialogs on first launch | ☐ | ☐ | ☐ | |

**Section 1 Score:** _____ / 5

---

## Section 2: Authentication (6 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 2.1 | Login screen displays correctly | ☐ | ☐ | ☐ | |
| 2.2 | Can login with valid technician credentials | ☐ | ☐ | ☐ | |
| 2.3 | Invalid credentials show clear error message | ☐ | ☐ | ☐ | |
| 2.4 | Password field masks input | ☐ | ☐ | ☐ | |
| 2.5 | "Remember me" checkbox works correctly | ☐ | ☐ | ☐ | |
| 2.6 | Logout works and returns to login screen | ☐ | ☐ | ☐ | |

**Section 2 Score:** _____ / 6

---

## Section 3: Guide Library (8 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 3.1 | All guides load and display correctly | ☐ | ☐ | ☐ | |
| 3.2 | Guide cards show: title, category, duration, difficulty | ☐ | ☐ | ☐ | |
| 3.3 | Search box filters guides correctly | ☐ | ☐ | ☐ | |
| 3.4 | Category filter works (Mechanical, Electrical, etc.) | ☐ | ☐ | ☐ | |
| 3.5 | Clear filters button resets all filters | ☐ | ☐ | ☐ | |
| 3.6 | Guides load in under 2 seconds | ☐ | ☐ | ☐ | |
| 3.7 | Scrolling is smooth (no lag or stuttering) | ☐ | ☐ | ☐ | |
| 3.8 | Clicking a guide navigates to step view | ☐ | ☐ | ☐ | |

**Section 3 Score:** _____ / 8

---

## Section 4: Step Navigation (10 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 4.1 | First step displays when guide opens | ☐ | ☐ | ☐ | |
| 4.2 | Step title and description are clear and readable | ☐ | ☐ | ☐ | |
| 4.3 | Images load correctly (if present) | ☐ | ☐ | ☐ | |
| 4.4 | Videos play correctly (if present) | ☐ | ☐ | ☐ | |
| 4.5 | "Next" button navigates to next step | ☐ | ☐ | ☐ | |
| 4.6 | "Previous" button navigates to previous step | ☐ | ☐ | ☐ | |
| 4.7 | Progress bar updates correctly | ☐ | ☐ | ☐ | |
| 4.8 | "Mark Complete" checkbox works | ☐ | ☐ | ☐ | |
| 4.9 | Safety warnings (if present) are clearly visible | ☐ | ☐ | ☐ | |
| 4.10 | Step navigation is fast (< 500ms per step) | ☐ | ☐ | ☐ | |

**Section 4 Score:** _____ / 10

---

## Section 5: Offline Functionality (7 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 5.1 | App works when network is disconnected | ☐ | ☐ | ☐ | |
| 5.2 | Offline banner displays when network is unavailable | ☐ | ☐ | ☐ | |
| 5.3 | Previously viewed guides work offline | ☐ | ☐ | ☐ | |
| 5.4 | Cached images display offline | ☐ | ☐ | ☐ | |
| 5.5 | Progress is saved locally while offline | ☐ | ☐ | ☐ | |
| 5.6 | Network reconnection updates UI correctly | ☐ | ☐ | ☐ | |
| 5.7 | No crashes when toggling network on/off | ☐ | ☐ | ☐ | |

**Section 5 Score:** _____ / 7

---

## Section 6: Accessibility (8 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 6.1 | All interactive elements accessible via keyboard | ☐ | ☐ | ☐ | |
| 6.2 | Tab order is logical and intuitive | ☐ | ☐ | ☐ | |
| 6.3 | Focus indicators are clearly visible | ☐ | ☐ | ☐ | |
| 6.4 | Screen reader announces all content correctly | ☐ | ☐ | ☐ | |
| 6.5 | Text is readable at default size (no squinting) | ☐ | ☐ | ☐ | |
| 6.6 | Font size setting works (Small/Medium/Large) | ☐ | ☐ | ☐ | |
| 6.7 | High contrast mode displays correctly | ☐ | ☐ | ☐ | |
| 6.8 | Touch targets are at least 44x44 pixels | ☐ | ☐ | ☐ | |

**Section 6 Score:** _____ / 8

---

## Section 7: Performance (6 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 7.1 | App launches in under 3 seconds (cold start) | ☐ | ☐ | ☐ | |
| 7.2 | Memory usage stays under 150MB when idle | ☐ | ☐ | ☐ | |
| 7.3 | No noticeable lag when navigating | ☐ | ☐ | ☐ | |
| 7.4 | Large guides (50+ steps) load smoothly | ☐ | ☐ | ☐ | |
| 7.5 | Scrolling is smooth at 60 FPS | ☐ | ☐ | ☐ | |
| 7.6 | App remains responsive after 1 hour of use | ☐ | ☐ | ☐ | |

**Section 7 Score:** _____ / 6

---

## Section 8: Error Handling (7 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 8.1 | App handles network interruption gracefully | ☐ | ☐ | ☐ | |
| 8.2 | Missing images show placeholder (not broken icon) | ☐ | ☐ | ☐ | |
| 8.3 | Database errors show user-friendly message | ☐ | ☐ | ☐ | |
| 8.4 | App doesn't crash when guide deleted mid-view | ☐ | ☐ | ☐ | |
| 8.5 | Low disk space warning appears appropriately | ☐ | ☐ | ☐ | |
| 8.6 | Corrupt data is detected and reported | ☐ | ☐ | ☐ | |
| 8.7 | All error messages are clear and actionable | ☐ | ☐ | ☐ | |

**Section 8 Score:** _____ / 7

---

## Section 9: Real-World Workflow (10 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 9.1 | Can complete full guide from start to finish | ☐ | ☐ | ☐ | |
| 9.2 | Progress saves correctly when app is closed | ☐ | ☐ | ☐ | |
| 9.3 | Can resume guide from last completed step | ☐ | ☐ | ☐ | |
| 9.4 | Can switch between multiple guides | ☐ | ☐ | ☐ | |
| 9.5 | App works in noisy factory environment (tested on floor) | ☐ | ☐ | ☐ | |
| 9.6 | Touch screen works with gloved hands | ☐ | ☐ | ☐ | |
| 9.7 | App readable in bright lighting (tested on floor) | ☐ | ☐ | ☐ | |
| 9.8 | App readable in dim lighting (tested on floor) | ☐ | ☐ | ☐ | |
| 9.9 | Device remains cool during extended use (no overheating) | ☐ | ☐ | ☐ | |
| 9.10 | Battery drain is acceptable for 8-hour shift | ☐ | ☐ | ☐ | |

**Section 9 Score:** _____ / 10

---

## Section 10: Updates & Maintenance (5 items)

| # | Test Item | Pass | Fail | N/A | Notes |
|---|-----------|------|------|-----|-------|
| 10.1 | Update notification appears when available | ☐ | ☐ | ☐ | |
| 10.2 | Update installs without data loss | ☐ | ☐ | ☐ | |
| 10.3 | Settings are preserved after update | ☐ | ☐ | ☐ | |
| 10.4 | Progress is preserved after update | ☐ | ☐ | ☐ | |
| 10.5 | App version number is visible in About/Settings | ☐ | ☐ | ☐ | |

**Section 10 Score:** _____ / 5

---

## Overall Score

| Section | Score | Max | Pass? |
|---------|-------|-----|-------|
| 1. Installation & Setup | _____ | 5 | ☐ |
| 2. Authentication | _____ | 6 | ☐ |
| 3. Guide Library | _____ | 8 | ☐ |
| 4. Step Navigation | _____ | 10 | ☐ |
| 5. Offline Functionality | _____ | 7 | ☐ |
| 6. Accessibility | _____ | 8 | ☐ |
| 7. Performance | _____ | 6 | ☐ |
| 8. Error Handling | _____ | 7 | ☐ |
| 9. Real-World Workflow | _____ | 10 | ☐ |
| 10. Updates & Maintenance | _____ | 5 | ☐ |
| **TOTAL** | **_____** | **72** | **☐** |

**Pass Criteria:**
- All critical tests (marked with ⚠) must pass
- Overall score ≥ 90% (65/72)
- No section below 80%

---

## Critical Issues Found

List any critical issues that must be fixed before release:

1. ____________________________________________________________

2. ____________________________________________________________

3. ____________________________________________________________

---

## High Priority Issues Found

List high priority issues that should be fixed:

1. ____________________________________________________________

2. ____________________________________________________________

3. ____________________________________________________________

---

## Medium/Low Priority Issues

List medium and low priority issues for future consideration:

1. ____________________________________________________________

2. ____________________________________________________________

3. ____________________________________________________________

---

## Technician Feedback

### What worked well?

____________________________________________________________

____________________________________________________________

____________________________________________________________

### What needs improvement?

____________________________________________________________

____________________________________________________________

____________________________________________________________

### Would you use this app in your daily work?

☐ Yes, definitely
☐ Yes, with some improvements
☐ Maybe
☐ No

**Reason:** ____________________________________________________

____________________________________________________________

---

## Tester Sign-Off

I have completed this field validation checklist to the best of my ability and accurately reported all findings.

**Signature:** _______________________  **Date:** ______________

---

## QA Team Review

**Reviewed by:** _______________________  **Date:** ______________

**Action Required:**

☐ Approve for release
☐ Fix critical issues and retest
☐ Needs major rework

**QA Notes:**

____________________________________________________________

____________________________________________________________

____________________________________________________________

---

**End of Field Validation Checklist**
