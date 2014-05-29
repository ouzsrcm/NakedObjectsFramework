// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Core.Util;

namespace NakedObjects.Core.Adapter {
    public class FileVersion : AbstractVersion, IEncodedToStrings {
        private static IClock clock;

        public FileVersion(string user)
            : this(user, clock.Ticks) {}

        public FileVersion(string user, long sequence)
            : base(user, new DateTime(sequence)) {}

        public FileVersion(string[] strings)
            : base(strings[0], new DateTime(long.Parse(strings[1]))) {}

        public static IClock Clock {
            set { clock = value; }
        }

        public virtual long Sequence {
            get { return time.GetValueOrDefault().Ticks; }
        }

        #region IEncodedToStrings Members

        public string[] ToEncodedStrings() {
            var helper = new StringEncoderHelper();

            helper.Add(user);
            helper.Add(time.GetValueOrDefault().Ticks);

            return helper.ToArray();
        }

        public string[] ToShortEncodedStrings() {
            return ToEncodedStrings();
        }

        #endregion

        public override bool Equals(IVersion other) {
            if (other is FileVersion) {
                return IsSameTime((FileVersion) other);
            }
            return false;
        }

        public override IVersion Next(string newUser, DateTime? newTime) {
            throw new NotImplementedException();
        }

        private bool IsSameTime(FileVersion other) {
            return time.GetValueOrDefault().Ticks == other.time.GetValueOrDefault().Ticks;
        }

        public override bool Equals(object obj) {
            if (obj is IVersion) {
                return Equals((IVersion) obj);
            }
            return false;
        }

        public override int GetHashCode() {
            return time.GetValueOrDefault().Ticks.GetHashCode();
        }

        public override string AsSequence() {
            return Convert.ToString(Sequence, 16);
        }

        public override string ToString() {
            var str = new AsString(this);
            str.Append("sequence", time.GetValueOrDefault().Ticks);
            str.Append("time", time);
            str.Append("user", user);
            return str.ToString();
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}