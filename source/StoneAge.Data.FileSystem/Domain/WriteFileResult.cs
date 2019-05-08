﻿using System.Collections.Generic;

namespace StoneAge.FileStore.Domain
{
    public class WriteFileResult
    {
        public bool HadError => ErrorMessages.Count > 0;

        public List<string> ErrorMessages { get; set; }
    }
}
