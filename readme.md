# Toutv Extra Downloader
This simple command-line tool will allow you to download individual episodes or whole tv shows from Radio-Canada's online service, tou.tv.

## Usage
### Login
Authenticates against the tou.tv oauth service provider. You *must* use this command the first time.

`toutv login -u user@domain.com -p password` 

### Fetch
Downloads a show from the service. Use the slug from the tou.tv website.

`http://ici.tou.tv/30-vies/S05E89` becomes `30-vies/S05E89`

`toutv fetch -m la-guerre-des-tuques`
`toutv fetch -m infoman/S15E23`

### Batch
Downloads all the available episodes from a show, skipping already downloaded files.

* `toutv batch --show 19-2`

## Requirements
* .NET 4.5
* ffmpeg

## Acknowledgements
* [Benjamin Vanheuverzwijn](https://github.com/bvanheu/) and all contributors to the [pytoutv](https://github.com/bvanheu/pytoutv) project
* [Fiddler](http://www.telerik.com/fiddler)