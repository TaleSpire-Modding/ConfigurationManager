# Configuration Manager

This plugin provides an update to the settings tab in TaleSpire. This allow users to update plugin configs at runtime without having to restart the entire applications.

## Usage

When you open the settings menu in TaleSpire, an extra 4th tab which gathers all plugin config settings to a place that you can alter them. The Following types are supported:
- Numbers
- Text
- Enums (as a dropdown)
- Keyboard Shortcuts (only displays, WIP)

## Changelog
- 0.13.0: Update for symbiote slab release
- 0.12.0: Update for symbiote UI in settings
- 0.11.0: Fix error being thrown on role switch in game.
- 0.10.0: Upgrade to Framework 4.8.0 and Nuget Dependency
- 0.9.10: include callbacks for plugins (this is not covered by Config Manager's Sentry)
- 0.9.8: removed exception throw that was left in 0.9.7
- 0.9.7: Attach version of config manager to sentry.
- 0.9.6: Implementing Sentry (opt-in).
- 0.9.5: Fixed bug where scene change would revert config settings
- 0.9.4: update category
- 0.9.3: use configEditor for string if declared as json.
- 0.9.2: fix text input alignment, updated logging, optimized performance.
- 0.9.1: Update icon (no change)
- 0.9.0: Alpha Release

## Shoutouts
Shoutout to my Patreons on https://www.patreon.com/HolloFox recognising your
mighty contribution to my caffeine addiction:
- John Fuller
- [Tales Tavern](https://talestavern.com/) - MadWizard