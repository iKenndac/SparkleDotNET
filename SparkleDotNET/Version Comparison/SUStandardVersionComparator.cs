using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SparkleDotNET {

    class SUStandardVersionComparator : SUVersionComparison {

        private enum SUCharacterType {
            kNumberType,
            kStringType,
            kPeriodType
        } ;

        static SUStandardVersionComparator sharedComparator;

        public static SUStandardVersionComparator SharedComparator() {
            if (sharedComparator == null) {
                sharedComparator = new SUStandardVersionComparator();
            }
            return sharedComparator;
        }

        private SUCharacterType TypeOfCharacter(string c) {

            if (String.IsNullOrWhiteSpace(c)) {
                return SUCharacterType.kStringType;
            } else if (c.Equals(".")) {
                return SUCharacterType.kPeriodType;
            } else if ("1234567890".Contains(c.Substring(0, 1))) {
                // ^That could be done less stupidly, I'm sure.  
                return SUCharacterType.kNumberType;
            } else {
                return SUCharacterType.kStringType;
            }
        }

        

        private ArrayList SplitVersionString(string version) {

            string character;
            string currentPart;
            Int32 i, n;
            SUCharacterType oldType, newType;
            ArrayList parts = new ArrayList();

            if (string.IsNullOrWhiteSpace(version)) {
                return parts;
            }

            currentPart = version.Substring(0, 1);
            oldType = TypeOfCharacter(currentPart);
            n = version.Length - 1;

            for (i = 1; i <= n; ++i) {
                character = version.Substring(i, 1);
                newType = TypeOfCharacter(character);
                if (oldType != newType || oldType == SUCharacterType.kPeriodType) {
                    // New segment!
                    parts.Add(currentPart);
                    currentPart = character;
                } else {
                    currentPart += character;
                }
                oldType = newType;
            }
            
            parts.Add(currentPart);
            return parts;
        }



        #region SUVersionComparison Members

        public int CompareVersionToVersion(string versionA, string versionB) {

            if (String.IsNullOrWhiteSpace(versionA) || String.IsNullOrWhiteSpace(versionB)) {
                return 0;
            }

            ArrayList partsA = SplitVersionString(versionA);
            ArrayList partsB = SplitVersionString(versionB);

            string partA, partB;
            Int32 i, n, intA, intB;
            SUCharacterType typeA, typeB;

            n = Math.Min(partsA.Count, partsB.Count);
            for (i = 0; i < n; ++i) {
                partA = (string)partsA[i];
                partB = (string)partsB[i];

                typeA = TypeOfCharacter(partA);
                typeB = TypeOfCharacter(partB);

                if (typeA == typeB) {
                    // Easily comparable
                    if (typeA == SUCharacterType.kNumberType) {
                        intA = Int32.Parse(partA);
                        intB = Int32.Parse(partB);

                        if (intA != intB) {
                            return intA.CompareTo(intB);
                        }
                    } else if (typeA == SUCharacterType.kStringType) {
                        int result = partA.CompareTo(partB);
                        if (result != 0) {
                            return result;
                        }
                    }

                    //NSOrderedAscending = -1, NSOrderedSame, NSOrderedDescending
                } else {
                    // Not the same - do some validity checking
                    if (typeA != SUCharacterType.kStringType && typeB == SUCharacterType.kStringType) {
                        // typeA wins
                        return 1;
                    } else if (typeA == SUCharacterType.kStringType && typeB != SUCharacterType.kStringType) {
                        // typeB wins
                        return -1;
                    } else {
                        // One is a number and the other is a period. The period is invalid
                        if (typeA == SUCharacterType.kNumberType) {
                            return 1;
                        } else {
                            return -1;
                        }
                    }
                }
            }

            // The versions are equal up to the point where they both still have parts
            // Lets check to see if one is larger than the other

            if (partsA.Count != partsB.Count) {

                // Yep. Lets get the next part of the larger
                // n holds the index of the part we want.

                string missingPart;
                SUCharacterType missingType;
                Int32 shorterResult, largerResult;

                if (partsA.Count > partsB.Count) {
                    missingPart = (string)partsA[n];
                    shorterResult = -1;
                    largerResult = 1;
                } else {
                    missingPart = (string)partsB[n];
                    shorterResult = 1;
                    largerResult = -1;
                }

                missingType = TypeOfCharacter(missingPart);
                // Check the type
                if (missingType == SUCharacterType.kStringType) {
                    // It's a string. Shorter version wins
                    return shorterResult;
                } else {
                    // It's a number/period. Larger version wins
                    return largerResult;
                }
            }

            // They're the same
            return 0;
        }

        #endregion
    }
}
