
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CSharpMetric
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async System.Threading.Tasks.Task fileChooserButton_ClickAsync()
        {
            //This button is simple enough, it opens a file and

            /// Increment by one for each new line.
            int linesOfCode = 1;

            // Lines of code whose functionality can be given to a prior line of code.
            int aestheticLinesOfCode = 0;

            // Unadjusted function points. For the sake of automation it will be assumed that all Function Points are of the lowest complexity.
            int functionPoints = 0;

            var fileOpener = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpener.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            fileOpener.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            fileOpener.FileTypeFilter.Add(".cs");
            fileOpener.FileTypeFilter.Add(".txt");
            Windows.Storage.StorageFile fileToOpen = await fileOpener.PickSingleFileAsync();
            if (fileToOpen != null)
            {
                fileChooserButton.Content = fileToOpen.DisplayName;
                //This gets the file as a list of strings, line by line. As a result we also get LOC in the broadest sense. 
                IList<string> fileLineByLine = await Windows.Storage.FileIO.ReadLinesAsync(fileToOpen);
                linesOfCode = fileLineByLine.Count;
                LOCTextbox.Text = linesOfCode.ToString();
                Boolean isAesthetic = false;
                int methodCount = getMethodCount(fileLineByLine);
                aestheticLOC.Text = aestheticCheck(fileLineByLine).ToString();
                unadjustedFP.Text = countFunctionPoint(fileLineByLine).ToString();

            }
            else
            {
                //User did not pick a file. They can click the button again.
            }
        }

        private int getMethodCount(IList<string> fileLineByLine)
        {

            Boolean multiLineCommentCheck = false;
            int linesOfCode = fileLineByLine.Count;
            int methodCount = 0;

            for (int lineParseCounter = 0; lineParseCounter <= linesOfCode; lineParseCounter++)
            {
                Boolean commentCheck = false;
                int normalCommentPos = -1;
                int accessModPos = -1;
                int starOpenCommentPos = -1;
                int starEndCommentPos = -1;
                //Set to true if a * / is found, update the main bool at the end of the loop
                Boolean starEndCommentBool = false;
                int methodDeclarePos = -1;
                int semicolonPos = -1;
                int openParPos = -1;
                int closeParPos = -1;
                //Case 1: Given string has no method declarations.
                //Case 2: Given string has a commented out method declaration.
                //Case 3: Given string has a method declaration that is commented out by a / * comment.
                //Case 4: Given string has a method declaration.
                //Case 5: Given string has a method declaration and a comment after the declaration.
                //check to make sure the line isn't commented out.

                //Check for method declarations. In C# all methods need an access modifier, a method name, a return value,
                //and method params. However, methods without a specified access modifier are set to private as default.
                //We'll assume all methods have a specified access modifier
                //A method has an access modifier, a (, a ), and a return value type.

                //First check for an access modifier
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("public") > -1)
                {
                    accessModPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("public");
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("private") > -1)
                {
                    accessModPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("private");
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("protected") > -1)
                {
                    accessModPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("protected");
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("internal") > -1)
                {
                    accessModPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("internal");
                }

                //Get ( and ) positions. It doesn't matter if we get positions of ( and ) on non method declaration lines since there will be no access modifier.
                openParPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("(");
                closeParPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf(")");

                //We check to see if the method could be commented out.
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("//") > -1)
                {
                    commentCheck = true;
                    normalCommentPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("//");
                }
                if (multiLineCommentCheck)
                {
                    commentCheck = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("/*") > -1)
                {
                    multiLineCommentCheck = true;
                    commentCheck = true;
                    starOpenCommentPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("/*");
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("*/") > -1)
                {
                    starEndCommentPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("*/");
                    starEndCommentBool = true;
                }


                //Check for () and access modifier
                if ((accessModPos > -1) && (openParPos > -1) && (closeParPos > -1))
                {
                    //Check for comments.
                    if (commentCheck)
                    {
                        if (multiLineCommentCheck)
                        {
                            if ((starEndCommentPos < accessModPos) && (starEndCommentPos > -1))
                            {
                                if (normalCommentPos > -1)
                                {
                                    if (normalCommentPos < closeParPos)
                                    {
                                        //Case 2
                                    }
                                    else
                                    {
                                        methodCount++;
                                    }
                                }
                                else
                                {
                                    methodCount++;
                                }
                            }
                        }
                        else
                        {
                            if (normalCommentPos < closeParPos)
                            {
                                //Case 2.
                            }
                            else
                            {
                                //Case 5
                                methodCount++;
                            }
                        }
                    }
                    else
                    {
                        //Case 4, increment.
                        methodCount++;
                    }
                }
                else
                {
                    //Case 1
                }
                if (starEndCommentBool)
                {
                    multiLineCommentCheck = false;
                }
            }
            return methodCount;
        }

        /// <summary>
        /// Determine number of functional lines of code.
        /// </summary>
        /// <param name="lineToCheck"></param>
        /// <returns></returns>
        private int aestheticCheck(IList<string> fileLineByLine)
        {
            Boolean multiLineCommentCheck = false;
            int linesOfCode = fileLineByLine.Count;
            int checkVal = 0;
            Boolean isFunctional = true;
            for (int lineParseCounter = 0; lineParseCounter <= linesOfCode; lineParseCounter++)
            {
                Boolean commentCheck = false;
                int normalCommentPos = -1;
                int starOpenCommentPos = -1;
                int starEndCommentPos = -1;
                //Set to true if a * / is found, update the main bool at the end of the loop
                Boolean starEndCommentBool = false;
                int semicolonPos = -1;
                int closeBracketPos = -1;
                
                //We check to see if the method could be commented out.
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("//") > -1)
                {
                    commentCheck = true;
                    normalCommentPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("//");
                }
                if (multiLineCommentCheck)
                {
                    commentCheck = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("/*") > -1)
                {
                    multiLineCommentCheck = true;
                    commentCheck = true;
                    starOpenCommentPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("/*");
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("*/") > -1)
                {
                    starEndCommentPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf("*/");
                    starEndCommentBool = true;
                }
                semicolonPos = fileLineByLine.ElementAt(lineParseCounter).IndexOf(";");

                char[] blankChars = { ' ', '}', '\n' };
                string[] nonBlankContents = fileLineByLine.ElementAt(lineParseCounter).Split(blankChars);
                Boolean blankCheck = true;

                //Determine if the line contains any characters besides spaces, close curlybraces and new lines.
                foreach(var word in nonBlankContents)
                {
                    if(word != "")
                    {
                        blankCheck = false;
                    }
                }

                //Case 1: Functional code, do not increment counter.
                //Case 2: Line is commented out.
                //Case 3: Line is part of a multiline comment.
                //Case 4: Line has a comment after functional code.
                //Case 5: Line has code after a multiline comment ends.
                //Case 6: No comment, but the line is composed of only "}, ' ', \n"
                if (blankCheck)
                {
                    //No code on the line, no functionality.
                    isFunctional = false;
                }
                else
                {
                    if (multiLineCommentCheck)
                    {
                        if(starOpenCommentPos > -1)
                        {
                            //Multicomment begins.
                            if((starOpenCommentPos > semicolonPos) || (starOpenCommentPos > closeBracketPos))
                            {
                                //Code is still functional, need to check for normal comments
                                if (commentCheck)
                                {
                                    if ((normalCommentPos > semicolonPos) || (normalCommentPos > closeBracketPos))
                                    {
                                        //Comment is after code, still functional.
                                    }
                                    else
                                    {
                                        isFunctional = false;
                                    }
                                }
                            }
                            else
                            {
                                isFunctional = false;
                            }
                        }
                        else
                        {
                            if(starEndCommentPos > -1)
                            {
                                if((starEndCommentPos > semicolonPos) || (starEndCommentPos > closeBracketPos))
                                {
                                    //Code is still functional, need to check for normal comments
                                    if (commentCheck)
                                    {
                                        if ((normalCommentPos > semicolonPos) || (normalCommentPos > closeBracketPos))
                                        {
                                            //Comment is after code, still functional.
                                        }
                                        else
                                        {
                                            isFunctional = false;
                                        }
                                    }
                                }
                                else
                                {
                                    isFunctional = false;
                                }
                            }
                            else
                            {
                                //Whole line is a comment.
                                isFunctional = false;
                            }
                        }
                    }
                    else if (commentCheck)
                    {
                        if((normalCommentPos > semicolonPos) || (normalCommentPos > closeBracketPos))
                        {
                            //Comment is after code, still functional.
                        }
                        else
                        {
                            isFunctional = false;
                        }
                    }
                    else
                    {
                        //No comments, line of code has functionality
                    }
                }

                if (isFunctional)
                {
                    //The code is functional. 
                }
                else
                {
                    checkVal++;
                    isFunctional = true;
                }
   
            }

            return fileLineByLine.Count - checkVal;
        }

        private int countFunctionPoint(IList<string> fileLineByLine)
        {
            //To count Unadjusted Function Points (UFP) I followed the following guidelines
            //1. I am only covering methods in System.IO
            //2. If a method could be seen as not being a UFP, it won't.
            //3. In the event that a method could be seen as being an either/or of two UFP types it will be assumed that it is of the type with the lower value. 
            //4. The application is seen as the file that is the input to this program.

            int functionPointCounter = 0;
            Boolean internalLogicFile = false;
            Boolean externalInterfaceFile = false;
            Boolean externalInput = false;
            Boolean externalOutput = false;
            Boolean externalInquiry = false;
            Boolean nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingy = false;
            int linesOfCode = fileLineByLine.Count;

            for (int lineParseCounter = 0; lineParseCounter <= linesOfCode; lineParseCounter++)
            {
                //Reset Booleans
                internalLogicFile = false;
                externalInterfaceFile = false;
                externalInput = false;
                externalOutput = false;
                externalInquiry = false;

                //Determine if line is an UFP. Only covering System.IO
                if(fileLineByLine.ElementAt(lineParseCounter).IndexOf("nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingy = true;
                }
                //BinaryReader
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".PeekChar(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Read(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadBoolean(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadByte(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadBytes(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadChar(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadChars(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadDecimal(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadDouble(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadInt16(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadInt32(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadInt64(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadSByte(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadSingle(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadString(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadUInt16(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadUInt32(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadUInt64(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }

                //BinaryWriter
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Write(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Write7BitEncodedInt(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Write7BitEncodedInt64(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //BufferedStream
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".BeginRead(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".BeginWrite(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".CopyTo(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".CopyToAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteByte(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //Directory
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".CreateDirectory(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Delete(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".EnumerateDirectories(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".EnumerateFiles(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".EnumerateFileSystemEntries(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Exists(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetCreationTime(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetCreationTimeUtc(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetCurrentDirectory(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    internalLogicFile = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetDirectories(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetDirectoryRoot(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetFiles(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetFileSystemEntries(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetLastAccessTime(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetLastAccessTimeUtc(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetLastWriteTime(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetLastWriteTimeUtc(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetLogicalDrives(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetParent(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Move(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetCreationTime(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetCreationTimeUtc(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetCurrentDirectory(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    internalLogicFile = true; 
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetLastAccessTime(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetLastAccessTimeUtc(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetLastWriteTime(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetLastWriteTimeUtc(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //Directory Info
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Create(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".CreateSubdirectory(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".EnumerateFileSystemInfos(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetFileSystemInfos(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".MoveTo(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetAccessControl(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetAccessControl(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //DriveInfo
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetDrives(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }

                //File
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".AppendAllLines(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".AppendAllLinesAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".AppendAllText(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".AppendAllTextAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".AppendText(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Copy(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Create(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".CreateText(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Exists(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetAttributes(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Open(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInterfaceFile = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OpenRead(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInterfaceFile = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OpenText(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInterfaceFile = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OpenWrite(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInterfaceFile = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadAllBytes(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf("ReadAllBytesAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadAllLines(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadAllLinesAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadAllText(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadAllTextAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadLines(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Replace(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".SetAttributes(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteAllBytes(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteAllBytesAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteAllLines(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteAllLinesAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteAllText(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteAllTextAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //FileInfo
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".CopyTo(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".CreateText(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //FileSystemWatcher
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OnChanged(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OnCreated(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OnDeleted(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OnError(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".OnRenamed(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInput = true;
                }

                //MemoryStream
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteTo(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //Path
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetFullPath(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetTempFileName(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    internalLogicFile = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".GetTempPath(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }

                //StreamReader
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadBlock(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadBlockAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadLine(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadLineAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadToEnd(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadToEndAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }

                //StreamWWriter
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteLine(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteLineAsync(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }

                //UnmanagedMemoryAccessor
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Read<T>(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".ReadArray<T>(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalInquiry = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".Write<T>(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                if (fileLineByLine.ElementAt(lineParseCounter).IndexOf(".WriteArray<T>(") > -1) //nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingyX01
                {
                    externalOutput = true;
                }
                //Increment counter where applicable.
                if (nickSuperDuperOverlyLongNamedFalsePositiveRemoverBooleanThingy)
                {
                    //Ignore false positives for this file.
                }
                else
                {
                    if (internalLogicFile)
                    {
                        functionPointCounter += 7;
                    }
                    if (externalInterfaceFile)
                    {
                        functionPointCounter += 5;
                    }
                    if (externalInput)
                    {
                        functionPointCounter += 3;
                    }
                    if (externalOutput)
                    {
                        functionPointCounter += 4;
                    }
                    if (externalInquiry)
                    {
                        functionPointCounter += 3;
                    }
                }
            }
                return functionPointCounter;
        }

        private void fileChooserButton_Click(object sender, RoutedEventArgs e)
        {
            fileChooserButton_ClickAsync();
        }
    }
}
