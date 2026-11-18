# X-Plane 12 Localizer Fixer

A cross-platform utility designed to correct misaligned ILS localizers in X-Plane 12.

The application uses runway geometry from X-Plane's `apt.dat` to calculate corrected localizer positions and courses, helping resolve localizer alignment issues caused by differences between airport scenery and navigation data.

Associated glideslope (GS) and DME records are preserved without modification.

## Download and Installation

1. Go to the [GitHub Releases](https://github.com/mason23zh/XPlaneLocalizerFixer/releases) page.
2. Download the appropriate release archive for your operating system (Windows, macOS, or Linux).
3. **Extract the downloaded archive** to any folder you prefer. You do not need to install the application into your X-Plane directory.
4. Open the extracted folder and run the `XPlaneLocalizerFixer` executable (`XPlaneLocalizerFixer.exe` on Windows).
5. The application will open a console window. Select **Select X-Plane 12 installation** from the menu to get started.

**Important:** Extract the entire archive before running the application. Do not run the executable directly from inside the compressed archive, and do not separate it from the other included files.


## How to Use

### 1. Select your X-Plane installation

Launch `XPlaneLocalizerFixer`.

From the main menu, select **Select X-Plane 12 installation root folder**.

A native folder selection dialog will open. Select the root directory of your X-Plane 12 installation.

For example:

`F:\SteamLibrary\steamapps\common\X-Plane 12`

The application will validate the selected installation before continuing.

### 2. Generate corrected navigation data

Once a valid installation has been selected, choose **Generate corrected nav data** from the main menu.

The application will:

1. Read airport runway data from `Global Scenery/Global Airports/Earth nav data/apt.dat`.
2. Read navigation data from `Custom Data/earth_nav.dat`, falling back to `Resources/default data/earth_nav.dat` if necessary.
3. Scan enabled custom scenery packages listed in `Custom Scenery/scenery_packs.ini`.
4. Identify airports with custom runway definitions and exclude them from correction.
5. Calculate corrected localizer positions and courses for eligible ILS approaches.
6. Record localizers that could not be corrected, including their rejection reasons.
7. Generate the corrected navigation data and exclusion report.

### 3. Review the generated files

The application writes two files to its `output` directory:

```text
output/
├── user_nav_generated.dat
└── excluded_airports.txt
```

**`user_nav_generated.dat`**

Contains corrected localizer records, along with their associated original glideslope and DME records.

**`excluded_airports.txt`**

Contains a detailed report of airports and individual ILS localizers excluded from correction.

The report includes two categories:

- **Custom scenery exclusions:** Airports excluded entirely because enabled custom scenery packages contain their own runway definitions.
- **Localizer correction rejections:** Individual ILS localizers that could not be corrected because of runway matching issues, safety limits, or invalid geometry.

For each rejected localizer, the report includes its airport ICAO, runway identifier, localizer identifier, and rejection reason. Heading deviation and lateral displacement measurements are included when available.

Localizers without an associated glideslope are outside the scope of the correction process and are not included in the rejection report.

The exclusion report is generated even when no localizer corrections are produced.

### 4. Install the corrected navigation data

**Important: The application does not automatically install or overwrite your X-Plane navigation data.**

1. Navigate to your X-Plane installation's `Custom Data` directory.
2. Back up your existing `user_nav.dat`, if present.
3. Open `output/user_nav_generated.dat` in a text editor.
4. Copy the generated navigation records into `Custom Data/user_nav.dat`.
5. Ensure the resulting file contains the appropriate X-Plane navigation-data header and only one final `99` terminator.
6. Restart X-Plane 12 to load the changes.

If you already have custom navigation records in `user_nav.dat`, merge the generated records carefully rather than replacing your existing file. Avoid introducing duplicate navigation records.

## Custom Scenery Exclusions

X-Plane installations often contain third-party airports with custom runway geometry.

To avoid interfering with these airports, Localizer Fixer examines the enabled scenery packages listed in `Custom Scenery/scenery_packs.ini`.

For each enabled package, the application looks for:

`X-Plane 12/Custom Scenery/Some-Custom-Scenery/Earth nav data/apt.dat`

If that file defines a land airport (row `1`) containing at least one runway record (row `100`), the airport identifier from row `1` is added to the exclusion list.

**All localizer corrections for that airport are skipped**, regardless of runway or scenery loading priority.

Disabled scenery packages and packages without qualifying airport definitions are ignored.

The excluded airport identifiers are written to `output/excluded_airports.txt` for inspection.




## Localizer Correction Rejections

Localizer Fixer records eligible ILS localizers that could not be corrected in `output/excluded_airports.txt`.

A localizer may be rejected for several reasons:

| Reason | Description |
|---|---|
| Runway not found | The runway identifier in `earth_nav.dat` does not match an available runway in `apt.dat`. |
| Heading deviation exceeded | The localizer course differs from the runway's true heading by at least the configured threshold (default: 1°). |
| Lateral displacement exceeded | The required lateral correction exceeds the configured maximum (default: 50 metres). |
| Invalid runway or localizer data | Invalid coordinates, course information, or runway geometry prevent correction. |
| Geodesic projection failure | A valid corrected position could not be calculated. |

### Runway Identifier Mismatches

Runway identifier mismatches may occur when navigation data is newer than the airport runway definitions included with X-Plane.

For example, newer navigation data may identify a runway as `22L`, while the corresponding runway in X-Plane's `apt.dat` is still identified as `22`.

The application does not automatically assume these identifiers refer to the same physical runway. Such localizers are skipped and reported as `RunwayNotFound` to avoid potentially incorrect corrections.

### Example Exclusion Report

```text
# X-Plane Localizer Fixer
# Airports and localizers excluded from correction

# Custom scenery exclusions
# Reason: Enabled custom scenery with runway definitions

KJFK
VHHX

# Localizer correction rejections

ZYTL RWY 10 (IZF)
  Reason: HeadinZYTL RWY 10 (IZF)
  Reason: Localizer heading differs from runway heading by at least 1 degree.
  Heading deviation: 2.765°g deviation meets or exceeds 1 degrees.

ZPPP RWY 04R (ITH)
  Reason: No matching runway found in apt.dat.

ZPPP RWY 22L (IPB)
  Reason: No matching runway found in apt.dat.

# Total custom scenery airports: 2
# Total rejected localizers: 3
```

The example is illustrative.

**Important:** A rejected localizer does not necessarily mean that the entire airport is excluded. Other eligible localizers at the same airport may still be successfully corrected.

Rejected localizers are not written to `user_nav_generated.dat`. X-Plane's original navigation records remain available for those approaches.


## Localizer Correction and Safety

Localizer Fixer uses runway coordinates from `apt.dat` to calculate runway centerlines and adjust eligible localizers.

The correction process:

- Matches each localizer with its associated airport and runway.
- Considers only supported ILS localizers with associated glideslope data.
- Calculates the runway's true heading.
- Moves the localizer laterally onto the runway centerline.
- Adjusts the localizer's encoded course to match the runway heading.
- Preserves the original localizer elevation.
- Preserves the associated glideslope and DME records without modifying their data.

To reduce the risk of modifying intentionally offset approaches or making excessive adjustments, the default safety limits are:

| Setting | Default |
|---|---|
| Maximum lateral displacement | 50 metres |
| Maximum heading difference | 1 degree |


Localizers that exceed these safety limits are skipped and recorded in `excluded_airports.txt`, along with their rejection reasons.


For example, intentionally offset ILS approaches such as ZYTL RWY 10 may exceed the heading deviation threshold. The application skips these approaches to preserve their published localizer geometry.

The lateral displacement limit provides an additional safeguard against modifying localizers that are significantly displaced from the runway centerline, even when their encoded course closely matches the runway heading.

These checks are intended to reduce risk, but they do not guarantee that every generated correction is appropriate. Review generated data before installing it.

## Configuration

Application settings are stored in `appsettings.json`.

```json
{
  "FilePaths": {
    "OutputDirectory": "output",
    "OutputFileName": "user_nav_generated.dat",
    "ExcludedAirportsFileName": "excluded_airports.txt"
  },
  "LocalizerCorrection": {
    "MaxLateralDeviationMeters": 50.0,
    "MaxHeadingDeviationDegrees": 1.0
  }
}
```

The correction safety limits and output filenames can be adjusted as needed.

The X-Plane installation directory is selected interactively and is not stored in `appsettings.json`.

## Disclaimer

This is an unofficial, community-developed utility and is not affiliated with or endorsed by Laminar Research or Navigraph.

Use generated navigation data at your own risk. Always back up existing files before making changes.

The application is designed to leave the original X-Plane installation data untouched. All generated navigation changes must be installed manually.
