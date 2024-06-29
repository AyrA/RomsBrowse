# RomsBrowse

ROM browser with built-in emulator and server side game saves

## Usage

Edit `appsettings.json` to point to an SQL server with an empty database,
then run however you prefer.
The application can run standalone, as IIS application, or as service.
To run it on a specific address, use `--urls` argument,
for example: `--urls http://localhost:54321`

## Initial Account

The first account you create will be the admin account.
To ensure nobody else can do this, you must copy the value of the "AdminToken"
from the "Settings" SQL table.

The registration form provides a field to past this value into.

## Configuration

When logged in as administrator,
you can use the account drop down menu in the top navbar to open the application settings,
there you can configure all aspects of the application.

## ROMs Directory

The ROM file directory needs to have a certain layout to be usable.
The application contains a help for how to set up the directory structure in the accounts menu.

## Server Side Save States

When signed in, save states are saved on the server,
and restored automatically when the emulator is run the next time.

A state is automatically uploaded whenever
the "Save State" button in the emulator menu bar is pressed.

## Server Side SRAM

Game memory that is used by the game for the regular save function
is uploaded and restored from the server in a similar fashion to save states.

The application detects automatically when the SRAM changes,
and then uploads it.
