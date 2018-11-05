# Xamarin.Android.RootDetection
Simple to use Xamarin.Android root checking library

## Getting started

```
using RootDevelop.RootDetection;

var rootChecker = new RootChecker(PackageManager);

if (rootChecker.IsRooted()) {
	// device is rooted, your action here
}

```
