
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

        private async System.Threading.Tasks.Task fileChooserButton_ClickAsync(object sender, RoutedEventArgs e)
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
                Boolean isAesthetic = false;
                int containsFP = 0;
                int methodCount = getMethodCount(fileLineByLine);
                aestheticCheck(fileLineByLine);

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

        private int detectFunctionPoint(string lineToCheck)
        {
            int functionPointCounter = 0;
            return functionPointCounter;
        }

    }
}
