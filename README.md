# xscreenshot
A reincarnation of the screenshot gem for fastlane that works for Xamarin.Forms apps


## install

xscreenshot is installed via homebrew (go find out how to set that up here: [link]), install xscreenshot via:

$ brew tap ecomerc/tap 
$ brew install xscreenshot


### add to fastlane

xscreenshot contains a fastlane plugin that can be installed into your fastlane project, goto your project and add the following into your fastlane/Pluginfile

gem "fastlane-plugin-xscreenshot_fl", path: "/usr/local/opt/xscreenshot"