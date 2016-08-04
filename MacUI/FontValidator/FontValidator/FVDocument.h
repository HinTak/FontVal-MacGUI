//
//  FVDocument.h
//  FontValidator
//
//  Created by Georg Seifert on 07.02.16.
//  Copyright © 2016 Georg Seifert. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@interface FVDocument : NSDocument

@property (nonatomic, weak) IBOutlet NSLayoutConstraint *optionHeightConstraint;
@property (nonatomic, weak) IBOutlet NSButton *validationButton;

- (IBAction)doValidate:(id)sender;

@end

