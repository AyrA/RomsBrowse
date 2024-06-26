# RomsBrowse

ROM browser with built-in emulator

## Usage

Edit `appsettings.json` to point to an SQL server with an empty database,
then run however you prefer.
The application can run standalone, as IIS application, or as service.
To run it on a specific address, use `--urls` argument,
for example: `--urls http://localhost:54321`

### Initial Account

The first account you create will be the admin account.
To ensure nobody else can do this, you must copy the value of the "AdminToken"
from the "Settings" SQL table.

The registration form provides a field to past this value into.

### Configuration

When logged in as administrator,
you can use the account drop down menu in the top navbar to open the application settings,
there you can configure all aspects of the application.

### ROMs Directory

The ROM file directory needs to have a certain layout to be usable.
The application contains a help for how to set up the directory structure in the menu.

**TODO: Server side save states**

**TODO: Server side game memory**
