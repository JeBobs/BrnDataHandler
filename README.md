# BrnDataHandler
#### I really need to rename this application sooner rather than later.
An application to help parse various data formats and properly identify/sort them, in the context of recovered Burnout builds. May abstract into a more general tool later on.

## Goals

### Recover
Looking to further parse each file type and identify properties such as: 
- Build config (External/Artist/Internal)
- Bundle type (TRK, APT, etc)
- Sound streams 

### Convert Asset Endian
Planning on writing functionality to convert the endianness of asset IDs, for instance:
- (BPR PC) 35_D3_52_CA <> (BP 360) CA_52_D3_35


## Contributing
If you would like to contribute, feel free to create a pull request with your feature (especially now that commands are modular! Come hop in here and write some goodness!). Any progress is welcome.
