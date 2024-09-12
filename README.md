# ContextMenuUploader

Command line tool to upload files using the Windows context menu.

## Notes

 - Windows only.
 - Admin privileges are required to register the context menu item.
 - The actual uploading is not implemented. The tool is a template for adding a context menu item to the Windows context menu.

## Installation

Just run the executable `ContextMenuUploader` from the command line or by double-clicking on it.
The tool will ask you to register the context menu item for files and folders.

### Command Line Options (alternative to the GUI)

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

## Configuration

Configure via the appsettings.json file: 

```json
{
  "Options":
  {
    "BatchTimeoutMs": 500,
    "MultipleInvokeLimit": 1000
  }
}
```

BatchTimeoutMs: The time in milliseconds to wait for additional invocations before processing the batch.

MultipleInvokeLimit: The maximum number of items that can be selected, for which the context menu tool is invoked.

----

Made with :heart: by [Spatial Focus](https://spatial-focus.net/)