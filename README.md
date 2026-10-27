# X-Plane 12 Localizer Fixer

A simple cross-platform utility for correcting misaligned ILS localizers in X-Plane 12.

The application uses runway data from `apt.dat` to calculate the correct localizer position and course, then generates corrected navigation data while preserving the associated glideslope (GS) and DME records.

Available for Windows, macOS, and Linux.

## How to Use

1. Download the appropriate version for your operating system from the [Releases](https://github.com/mason23zh/XPlaneLocalizerFixer/releases) page and extract it.
2. Copy the following files from your X-Plane 12 installation into the application's `input` folder:
    - **apt.dat** — From `Global Scenery/Global Airports/Earth nav data/`
    - **earth_nav.dat** — From `Custom Data/` (or `Resources/default data/` if you don't use custom navigation data).
3. Run `XPlaneLocalizerFixer` and select **Generate corrected nav data**.
4. Open the generated `output/user_nav_generated.dat` file in a text editor.
5. Copy the navigation data entries into your existing `user_nav.dat` file, located in X-Plane 12's `Custom Data` folder. Keep only one file header and one final `99` terminator.
6. Restart X-Plane 12 to load the updated navigation data.

### Important: Back Up Your Files

**Always make a backup of your existing `user_nav.dat` before making any changes.**

If you already have custom navigation data in `user_nav.dat`, do not replace the entire file. Merge the generated navigation data entries into your existing file to avoid losing your custom entries.

The application does not modify files inside your X-Plane installation. All changes must be installed manually.

## Configuration

You can adjust the input/output directories and correction safety limits in `appsettings.json`.

By default, localizers requiring more than 50 metres of lateral movement or having a heading difference of 1 degree or greater are skipped to help avoid modifying intentionally offset approaches.
