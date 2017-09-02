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
	
}
@property (nonatomic, weak) IBOutlet NSWindow *preferencesWindow;
@property (nonatomic, weak) IBOutlet SUUpdater *updater;
@end
