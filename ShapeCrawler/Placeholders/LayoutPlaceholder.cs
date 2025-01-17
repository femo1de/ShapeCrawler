﻿using DocumentFormat.OpenXml;
using ShapeCrawler.Extensions;
using ShapeCrawler.Shared;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.Placeholders
{
    /// <summary>
    ///     Represents a placeholder located on a Slide Layout.
    /// </summary>
    internal class LayoutPlaceholder : Placeholder
    {
        private readonly LayoutShape layoutShape;

        private LayoutPlaceholder(P.PlaceholderShape pPlaceholderShape, LayoutShape layoutShape)
            : base(pPlaceholderShape)
        {
            this.layoutShape = layoutShape;
            this.layoutReferencedShape = new ResettableLazy<Shape>(this.GetReferencedShape);
        }

        internal static LayoutPlaceholder Create(OpenXmlCompositeElement pShapeTreeChild, LayoutShape layoutShape)
        {
            P.PlaceholderShape pPlaceholderShape =
                pShapeTreeChild.ApplicationNonVisualDrawingProperties().GetFirstChild<P.PlaceholderShape>();
            if (pPlaceholderShape == null)
            {
                return null;
            }

            return new LayoutPlaceholder(pPlaceholderShape, layoutShape);
        }

        private Shape GetReferencedShape()
        {
            var shapes = this.layoutShape.SlideLayoutInternal.SlideMasterInternal.ShapesInternal;

            return shapes.GetReferencedShapeOrDefault(this.PPlaceholderShape);
        }
    }
}