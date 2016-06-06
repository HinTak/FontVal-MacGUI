//
//  AppDelegate.m
//  FontValidator
//
//  Created by Georg Seifert on 07.02.16.
//  Copyright © 2016 Georg Seifert. All rights reserved.
//

#import "AppDelegate.h"

#import <Sparkle/SUUpdater.h>

@interface AppDelegate ()

@end

@implementation AppDelegate

+ (void)initialize {
	NSUserDefaults *Defaults = [NSUserDefaults standardUserDefaults];
	[Defaults registerDefaults:@{@"check_BASE": @YES,
								 @"check_CBDT": @YES,
								 @"check_CBLC": @YES,
								 @"check_CFF_": @YES,
								 @"check_cmap": @YES,
								 @"check_COLR": @YES,
								 @"check_CPAL": @YES,
								 @"check_cvt_": @YES,
								 @"check_DSIG": @YES,
								 @"check_EBDT": @YES,
								 @"check_EBLC": @YES,
								 @"check_EBSC": @YES,
								 @"check_fpgm": @YES,
								 @"check_gasp": @YES,
								 @"check_GDEF": @YES,
								 @"check_glyf": @YES,
								 @"check_GPOS": @YES,
								 @"check_GSUB": @YES,
								 @"check_hdmx": @YES,
								 @"check_head": @YES,
								 @"check_hhea": @YES,
								 @"check_hmtx": @YES,
								 @"check_JSTF": @YES,
								 @"check_kern": @YES,
								 @"check_loca": @YES,
								 @"check_LTSH": @YES,
								 @"check_MATH": @YES,
								 @"check_maxp": @YES,
								 @"check_name": @YES,
								 @"check_OS/2": @YES,
								 @"check_PCLT": @YES,
								 @"check_post": @YES,
								 @"check_prep": @YES,
								 @"check_SVG_": @YES,
								 @"check_VDMX": @YES,
								 @"check_vhea": @YES,
								 @"check_vmtx": @YES,
								 @"check_VORG": @YES,
								 @"raster_tests": @NO}];
}

- (void)awakeFromNib {
	_updater = [[SUUpdater alloc] init];
	NSMenu *mainMenu = [[NSApplication sharedApplication] mainMenu];
	NSMenu *SubMenu = [[mainMenu itemAtIndex:0] submenu];
	NSMenuItem *MenuItem = [[NSMenuItem alloc] initWithTitle:NSLocalizedString(@"Check for Updates...", @"") action:@selector(checkForUpdates:) keyEquivalent:@""];
	[MenuItem setTarget:_updater];
	[SubMenu insertItem:MenuItem atIndex:2];
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
	// Insert code here to initialize your application
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
	// Insert code here to tear down your application
}

@end
