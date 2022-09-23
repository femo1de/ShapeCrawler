﻿using System.Drawing;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using ShapeCrawler.AutoShapes;
using ShapeCrawler.Exceptions;
using ShapeCrawler.Shared;
using A = DocumentFormat.OpenXml.Drawing;

// ReSharper disable CheckNamespace
namespace ShapeCrawler
{
    /// <summary>
    ///     Represents text frame.
    /// </summary>
    public interface ITextFrame
    {
        /// <summary>
        ///     Gets collection of paragraphs.
        /// </summary>
        IParagraphCollection Paragraphs { get; }

        /// <summary>
        ///     Gets or sets text.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        ///     Gets AutoFit type.
        /// </summary>
        SCAutoFitType AutoFitType { get; }

        /// <summary>
        ///     Gets a value indicating whether text frame can be changed.
        /// </summary>
        bool CanChange { get; }
    }

    internal class TextFrame : ITextFrame
    {
        private readonly ResettableLazy<string> text;
        private readonly ResettableLazy<ParagraphCollection> paragraphs;
        private readonly bool canChange;

        internal TextFrame(ITextFrameContainer frameContainer, TypedOpenXmlCompositeElement textBodyElement, bool canChange)
        {
            this.TextFrameContainer = frameContainer;
            this.TextBodyElement = textBodyElement;
            this.canChange = canChange;
            this.text = new ResettableLazy<string>(this.GetText);
            this.paragraphs = new ResettableLazy<ParagraphCollection>(this.GetParagraphs);
        }

        public IParagraphCollection Paragraphs => this.paragraphs.Value;

        public string Text
        {
            get => this.text.Value;
            set => this.SetText(value);
        }

        public SCAutoFitType AutoFitType => this.GetAutoFitType();

        public bool CanChange => this.canChange;

        internal ITextFrameContainer TextFrameContainer { get; }

        internal OpenXmlCompositeElement? TextBodyElement { get; }

        internal void ThrowIfRemoved()
        {
            this.TextFrameContainer.ThrowIfRemoved();
        }

        private ParagraphCollection GetParagraphs()
        {
            return new ParagraphCollection(this);
        }

        private SCAutoFitType GetAutoFitType()
        {
            if (this.TextBodyElement == null)
            {
                return SCAutoFitType.None;
            }

            var aBodyPr = this.TextBodyElement.GetFirstChild<A.BodyProperties>();
            if (aBodyPr!.GetFirstChild<A.NormalAutoFit>() != null)
            {
                return SCAutoFitType.Shrink;
            }

            if (aBodyPr.GetFirstChild<A.ShapeAutoFit>() != null)
            {
                return SCAutoFitType.Resize;
            }

            return SCAutoFitType.None;
        }

        private void SetText(string newText)
        {
            if (!this.CanChange)
            {
                throw new PlaceholderCannotBeChangedException();
            }

            var baseParagraph = this.Paragraphs.FirstOrDefault(p => p.Portions.Any());
            if (baseParagraph == null)
            {
                baseParagraph = this.Paragraphs.First();
                baseParagraph.AddPortion(newText);
            }

            var removingParagraphs = this.Paragraphs.Where(p => p != baseParagraph);
            this.Paragraphs.Remove(removingParagraphs);

            if (this.AutoFitType == SCAutoFitType.Shrink)
            {
                var popularPortion = baseParagraph.Portions.GroupBy(p => p.Font.Size).OrderByDescending(x => x.Count())
                    .First().First();
                var fontFamilyName = popularPortion.Font.Name;
                var fontSize = popularPortion.Font.Size;
                var stringFormat = new StringFormat { Trimming = StringTrimming.Word };
                var shape = this.TextFrameContainer.Shape;
                var bm = new Bitmap(shape.Width, shape.Height);
                using var graphic = Graphics.FromImage(bm);
                const int margin = 7;
                var rectangle = new Rectangle(margin, margin, shape.Width - 2 * margin, shape.Height - 2 * margin);
                var availSize = new SizeF(rectangle.Width, rectangle.Height);

                int charsFitted;
                do
                {
                    var font = new Font(fontFamilyName, fontSize);
                    graphic.MeasureString(newText, font, availSize, stringFormat, out charsFitted, out _);
                    fontSize--;
                } while (newText.Length != charsFitted);

                var paragraphInternal = (SCParagraph)baseParagraph;
                paragraphInternal.SetFontSize(fontSize);
            }

            baseParagraph.Text = newText;
        }

        private string GetText()
        {
            if (this.TextBodyElement == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append(this.Paragraphs[0].Text);

            // If the number of paragraphs more than one
            var numPr = this.Paragraphs.Count;
            var index = 1;
            while (index < numPr)
            {
                sb.AppendLine();
                sb.Append(this.Paragraphs[index].Text);

                index++;
            }

            return sb.ToString();
        }
    }
}