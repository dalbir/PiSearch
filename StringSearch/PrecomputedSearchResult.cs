﻿/*
 * PiSearch
 * Precompted Search Result - the precomputed result for a search of a specific string in a specific set of digits
 * By Josh Keegan 28/01/2015
 * Last Edit 09/06/2016
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringSearch
{
    public class PrecomputedSearchResult
    {
        //Public variables
        public readonly long MinSuffixArrayIdx; //Inclusive
        public readonly long MaxSuffixArrayIdx; //Exclusive

        public PrecomputedSearchResult(long minSuffixArrayIdx, long maxSuffixArrayIdx)
        {
            //Validation
            if(minSuffixArrayIdx < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minSuffixArrayIdx), "must be >= 0");
            }

            if(maxSuffixArrayIdx < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSuffixArrayIdx), "must be >= 0");
            }

            if(maxSuffixArrayIdx < minSuffixArrayIdx)
            {
                throw new ArgumentException("maxSuffixArrayIdx must be >= minSuffixArrayIdx");
            }

            MinSuffixArrayIdx = minSuffixArrayIdx;
            MaxSuffixArrayIdx = maxSuffixArrayIdx;
        }
    }
}
