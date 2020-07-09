//
//  FVDocument.m
//  FontValidator
//
//  Created by Georg Seifert on 07.02.16.
//  Copyright © 2016 Georg Seifert. All rights reserved.
//

#import "FVDocument.h"

#define OptionViewHeight 310

@interface FVDocument ()

@end

@implementation FVDocument

- (void)awakeFromNib {
	if ([[NSUserDefaults standardUserDefaults] boolForKey:@"OptionsVisible"]) {
		[_optionHeightConstraint setConstant:OptionViewHeight];
	}
	else {
		[_optionHeightConstraint setConstant:0];
	}
}

+ (BOOL)autosavesInPlace {
	return NO;
}

- (NSString *)windowNibName {
	return @"Document";
}

- (NSData *)dataOfType:(NSString *)typeName error:(NSError **)outError {
	// Insert code here to write your document to data of the specified type. If outError != NULL, ensure that you create and set an appropriate error when returning nil.
	// You can also choose to override -fileWrapperOfType:error:, -writeToURL:ofType:error:, or -writeToURL:ofType:forSaveOperation:originalContentsURL:error: instead.
	[NSException raise:@"UnimplementedMethod" format:@"%@ is unimplemented", NSStringFromSelector(_cmd)];
	return nil;
}

- (BOOL)readFromData:(NSData *)data ofType:(NSString *)typeName error:(NSError **)outError {
	return YES;
}

- (NSArray *)_ignoreTablesList {
	NSArray *tableKeys = @[@"BASE", @"CBDT", @"CBLC", @"CFF_", @"cmap", @"COLR", @"CPAL", @"cvt_", @"DSIG", @"EBDT", @"EBLC", @"EBSC", @"fpgm", @"gasp", @"GDEF", @"glyf", @"GPOS", @"GSUB", @"hdmx", @"head", @"hhea", @"hmtx", @"JSTF", @"kern", @"loca", @"LTSH", @"MATH", @"maxp", @"name", @"OS/2", @"PCLT", @"post", @"prep", @"SVG_", @"VDMX", @"vhea", @"vmtx", @"VORG"];
	
	NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
	NSMutableArray *ignoredTables = [NSMutableArray array];
	for (NSString *tableKey in tableKeys) {
		if (![defaults boolForKey:[NSString stringWithFormat:@"check_%@", tableKey]]) {
			[ignoredTables addObject:[tableKey stringByReplacingOccurrencesOfString:@"_" withString:@" "]];
		}
	}
	return ignoredTables;
}

- (IBAction)doValidate:(id)sender {
	[_validationButton setTitle:@"Validating"];
	[_validationButton setEnabled:NO];
	[self performSelectorInBackground:@selector(_validate) withObject:nil];
}

- (void)finishValidation:(NSString *)result {
	[_validationButton setTitle:@"Validate"];
	[_validationButton setEnabled:YES];
	if ([result length] > 0) {
		NSDictionary *errorDetail = @{NSLocalizedDescriptionKey : @"Something went wrong:", NSLocalizedRecoverySuggestionErrorKey : result};
		NSError *error = [NSError errorWithDomain:@"fontVal" code:1 userInfo:errorDetail];
		[self presentError:error];
	}
}

- (void)_validate {
	
	NSString *fontValBinPath = [[NSBundle mainBundle] pathForResource:@"FontValidator" ofType:@""];
	NSMutableArray *Arguments = [@[@"-file", [[self fileURL] path], @"-report-in-font-dir", @"-quiet"] mutableCopy];
	NSArray *ignoreTables = [self _ignoreTablesList];
	if ([ignoreTables count] > 0) {
		for (NSString *ignoreTable in ignoreTables) {
			[Arguments addObject:@"-table"];
			[Arguments addObject:ignoreTable];
		}
	}
	if (![[NSUserDefaults standardUserDefaults] boolForKey:@"raster_tests"]) {
		[Arguments addObject:@"-no-raster-tests"];
	}

	NSString *Result = [self callCommand:fontValBinPath withArguments:Arguments currentDirectory:nil];
	

	Result = [Result stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
	if ([Result hasSuffix:@"Reports are ready!"]) {
		[self performSelectorOnMainThread:@selector(finishValidation	:) withObject:nil waitUntilDone:NO];
		NSRange startRange = [Result rangeOfString:@"Complete: "];
		NSRange endRange = [Result rangeOfString:@".report.xml"];
		NSString *reportPath = [Result substringWithRange:NSMakeRange(NSMaxRange(startRange), NSMaxRange(endRange) - NSMaxRange(startRange))];
		reportPath = [reportPath stringByReplacingOccurrencesOfString:@".report.xml" withString:@".report.html"];
		NSURL *reportURL = [NSURL fileURLWithPath:reportPath];
		[[NSWorkspace sharedWorkspace] openURL:reportURL];
	}
	else {
		[self performSelectorOnMainThread:@selector(finishValidation	:) withObject:Result waitUntilDone:NO];
	}
}

- (IBAction)toggleOptionHeight:(id)sender {
	if ([_optionHeightConstraint constant] > 0) {
		[[NSUserDefaults standardUserDefaults] setBool:NO forKey:@"OptionsVisible"];
		[[_optionHeightConstraint animator] setConstant:0];
	}
	else {
		[[NSUserDefaults standardUserDefaults] setBool:YES forKey:@"OptionsVisible"];
		[[_optionHeightConstraint animator] setConstant:OptionViewHeight];
	}
}

- (NSString *)callCommand:(NSString *)Command withArguments:(NSArray *)Arguments currentDirectory:(NSString *)CurrentDirectory {
	if (Command) {
		NSTask *task;
		task = [[NSTask alloc] init];
		[task setLaunchPath:Command];
		if (CurrentDirectory) {
			[task setCurrentDirectoryPath:CurrentDirectory];
		}
		if (Arguments) {
			[task setArguments:Arguments];
		}
		NSMutableDictionary *environment = [[task environment] mutableCopy];
		if (!environment) {
			environment = [NSMutableDictionary dictionary];
		}
		NSString *dyldLibraryPath = environment[@"DYLD_LIBRARY_PATH"];
		if (!dyldLibraryPath) {
			dyldLibraryPath = @"";
		}
		NSString *libraryPath = [Command stringByDeletingLastPathComponent];
		if ([dyldLibraryPath rangeOfString:libraryPath].location == NSNotFound) {
			if ([dyldLibraryPath length] > 0) {
				dyldLibraryPath = [dyldLibraryPath stringByAppendingFormat:@":%@", libraryPath];
			}
			else {
				dyldLibraryPath = libraryPath;
			}
		}
		environment[@"DYLD_LIBRARY_PATH"] = dyldLibraryPath;
		[task setEnvironment:environment];
		NSPipe *Pipe = [NSPipe pipe];
		[task setStandardOutput:Pipe];
		[task setStandardError:Pipe];
		
		NSFileHandle *file;
		file = [Pipe fileHandleForReading];
		[task launch];
		
		NSData *Data;
		@try {
			Data = [file readDataToEndOfFile];
		}
		@catch (NSException *e) {
			NSLog(@"e: %@", e);
		}
		NSString *Result = [[NSString alloc] initWithData:Data encoding:NSUTF8StringEncoding];
		if ([Data length] > 0 && [Result length] == 0) {
			Result = [[NSString alloc] initWithData:Data encoding:NSASCIIStringEncoding];
		}
		return Result;
	}
	return nil;
}

@end
