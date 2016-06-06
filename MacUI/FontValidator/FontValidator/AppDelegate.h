//
//  AppDelegate.h
//  FontValidator
//
//  Created by Georg Seifert on 07.02.16.
//  Copyright © 2016 Georg Seifert. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@class SUUpdater;

@interface AppDelegate : NSObject <NSApplicationDelegate> {
	SUUpdater *_updater;
}


@end

