# Font-Validator

Font Validator is a tool for testing fonts prior to release. 
It was initially developed by Microsoft, to ensure that fonts meet Microsoft's high quality standards and perform exceptionally well on Microsoft's platform.

In 2015 the source code was published under the MIT license ([see release discussion](http://typedrawers.com/discussion/1222/microsoft-font-validator-lives))

## Usage

`FontVal.exe` is the GUI, and `FontValidator.exe` shows usage and example if run without arguments; both should be self-explanatory. 
Prepend with `mono` if runs on non-Windows systems.

The GUI's built-in help requires a CHM viewer, which defaults to [chmsee](https://github.com/jungleji/chmsee) on GNU+Linux, or via env variable `MONO_HELP_VIEWER` 

The GUI on X11/mono needs the env variable `MONO_WINFORMS_XIM_STYLE=disabled` set to work around [Bug 28047 - Forms on separare threads -- Fatal errors/crashes](https://bugzilla.xamarin.com/show_bug.cgi?id=28047)

## Binary Downloads

Since Release 2.0, binaries (`*.dmg` for Mac OS X, `*-bin-net2.zip` or `*-bin-net4.zip` for MS .NET/mono) are available from
[Binary Downloads](https://sourceforge.net/projects/hp-pxl-jetready/files/Microsoft%20Font%20Validator/).
From Release 2.1 onwards, gpg-signed binaries for Ubuntu Linux are also available. There is an additional and simplified location for
Binary downloads at the [Releases link above this page](https://github.com/HinTak/Font-Validator/releases).

Please consider [donating to the effort](https://sourceforge.net/p/hp-pxl-jetready/donate/), if you use the binaries.

## Build Instructions

[Build Instructions](https://github.com/HinTak/Font-Validator/wiki/Build-Instructions)

## Roadmap

### Missing/broken Parts

As of Release 2.0 (July 18 2016), all the withheld parts not released by Microsoft were re-implemented.
Release 2.0 run well on non-windows, and is substantially faster also.
Existing users of the increasingly dated 1.0 release from 2003 are encouraged to upgrade.
There are a number of known disagreements and issues which are gradually being filed and addressed.
[README-hybrid.txt](README-hybrid.txt) is now of historical interests only.

The [FontVal 2.2 Roadmap](https://github.com/HinTak/Font-Validator/wiki/Two-years-on,-and-2.2-Roadmap).

* The DSIG test (DSIG_VerifySignature) does not validate trusted certificate chain yet.

* Many post-2nd (i.e. 2009) edition changes, such as CBLC/CBDT and other new tables.

See [README-extra.txt](README-extra.txt) for a list of other interesting or non-essential tasks.

### Caveats

The 3 Rasterer-dependent metrics tests (LTSH/HDMX/VDMX) with a FreeType backend are known to behave somewhat differently compared to the MS Font Scaler backend. 
In particular:

HDMX: differ by up to two pixels (i.e. either side)

LTSH: FreeType backend shows a lot more non-linearity than an MS backend; the result with MS backend should be a sub-set of FreeType's, however.

VDMX: The newer code has a built-in 10% tolerance, so the newer FreeType backend result should be a sub-set of (the older) MS result. Also, note that MS 2003 binary seems to be wrong for non-square resolutions, so results differ by design.

On the other hand, the FreeType backend is up to 5x faster (assuming single-thread), and support CFF rastering. It is not known whether the MS backend is multi-threaded, but the FreeType backend is currently single-threaded.

### Annoyances

Table order is case-insensitive sorted in GUI, but case-sensitive sorted in output, both should be sorted consistently.

GUI allows in-memory reports, so CMD does not warn nor abort when output location is invalid, and wastes time producing no output.
Only `-report-dir` aborts on that; no workaround to `-report-in-font-dir` nor temp dir yet.
