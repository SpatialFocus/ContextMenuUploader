# ContextMenuUploader

Command line tool to upload files using the Windows context menu.

__Note:__ Windows only. The actual uploading is not implemented. The tool is a template for adding a context menu item to the Windows context menu.

## Installation

Register the context menu item for files and folders.

```bash
ContextMenuUploader --register
```

Unregister the context menu item for files and folders.

```bash
ContextMenuUploader --unregister
```

## Usage

Right-click on a file or folder and select "Upload to web service" from the context menu.

## Debug

Debug information is written to the file `ContextMenuUploader.log` in the directory `C:\temp`, if exists.

----

Made with :heart: by [Spatial Focus](https://spatial-focus.net/)