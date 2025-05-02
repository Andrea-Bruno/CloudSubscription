#!/bin/bash

INSTALL_PATH="/usr/share/cloud-subscription"

if (( $EUID != 0 )); then
    echo "WARNING: Superuser is required!"
    exit
fi

# copy files to working directory
if [ "$PWD" != "$INSTALL_PATH" ]; then
    mkdir -p "$INSTALL_PATH"
    chmod 700 "$INSTALL_PATH"
    cp -R . "$INSTALL_PATH"
    chmod 700 "$INSTALL_PATH/install.sh"
    rm -rf .
    cd "$INSTALL_PATH"
    ./install.sh
    exit
fi

# DOTNET runtime
# NOTE: You can download the runtime from here: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
#       and unzip the *.tga.gz file in home/$YourUser/.dotnet (or in a different position)

# Download the installer:
if which curl >/dev/null ; then
    curl -L -O https://dot.net/v1/dotnet-install.sh
elif which wget >/dev/null ; then
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
else
    echo "Cannot download, neither wget or curl is available!"
    exit
fi

DOTNET_PATH="/usr/share/dotnet"
mkdir -p $DOTNET_PATH
chmod 755 $DOTNET_PATH

chmod +x ./dotnet-install.sh
# info https://learn.microsoft.com/it-it/dotnet/core/tools/dotnet-install-script
./dotnet-install.sh --channel 9.0 --runtime aspnetcore --install-dir $DOTNET_PATH

#start the application
$DOTNET_PATH/dotnet CloudSubscription.dll