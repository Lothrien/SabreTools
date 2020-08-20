﻿namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Represents a blank set from an input DAT
    /// </summary>
    public class Blank : DatItem
    {
        #region Constructors

        /// <summary>
        /// Create a default, empty Archive object
        /// </summary>
        public Blank()
        {
            Name = string.Empty;
            ItemType = ItemType.Blank;
        }

        #endregion

        #region Cloning Methods

        public override object Clone()
        {
            return new Blank()
            {
                Name = this.Name,
                ItemType = this.ItemType,
                DupeType = this.DupeType,

                PartName = this.PartName,
                PartInterface = this.PartInterface,
                Features = this.Features,
                AreaName = this.AreaName,
                AreaSize = this.AreaSize,

                Machine = this.Machine.Clone() as Machine,
                Source = this.Source.Clone() as Source,
                Remove = this.Remove,
            };
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            // If we don't have a blank, return false
            if (ItemType != other.ItemType)
                return false;

            // Otherwise, treat it as a Blank
            Blank newOther = other as Blank;

            // If the archive information matches
            return (Machine == newOther.Machine);
        }

        #endregion
    }
}
