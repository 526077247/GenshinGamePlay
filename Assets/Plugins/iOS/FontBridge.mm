extern "C" {
    //从C#调用
    char* __NT_GetSystemFonts()
    {
        NSFileManager *fileManager = [NSFileManager defaultManager];
        NSString *home;
        home = @"/System/Library/Fonts/";

        NSDirectoryEnumerator *myDirectoryEnumerator = [fileManager enumeratorAtPath:home];;
        NSMutableSet *files = [[NSMutableSet alloc] init];
        for (NSString *path in myDirectoryEnumerator.allObjects) {

            [files addObject:[NSString stringWithFormat:@"%@%@", home, path]];

        }

        NSData *jsonData = [NSJSONSerialization dataWithJSONObject: [files allObjects]
                                                                options:NSJSONWritingPrettyPrinted
                                                                    error:nil];
        if ([jsonData length] > 0 )
        {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData
                                                            encoding:NSUTF8StringEncoding];

            return __makeCString(jsonString);
        }

        return __makeCString(@"");
    }
}