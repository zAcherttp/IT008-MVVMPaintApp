﻿using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Eraser(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public int BrushSize { get; set; } = 50;
        public const int PREVIEW_STROKE_REGION_PADDING = 10;

        public override void OnMouseDown(object sender, MouseEventArgs e, Point p)
        {
            
            base.OnMouseDown(sender, e, p);
            if (!IsValidDrawingState()) return;
            CurrentStrokeRegion = new Rect(p, new Size(1, 1));

            DrawPoint(p, Colors.Transparent);
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {
            
            DrawPreview(p, ProjectManager.CurrentProject.Background);

            if (!IsDrawing || !IsValidDrawingState()) return;

            float distance = CalculateDistance(LastPoint, p);
            if (distance < MIN_INTERP_DISTANCE) return;

            DrawLine(LastPoint, p, Colors.Transparent);
            LastPoint = p;
        }

        public override void OnMouseUp(object sender, MouseEventArgs e, Point imagePoint)
        {
            if (ProjectManager.StrokeLayer != null && CurrentStrokeRegion != null)
            {
                // Add history entry

                //BlitStrokeLayer();
            }

            ProjectManager.StrokeLayer.Clear(Colors.Transparent);
            ProjectManager.InvalidateRegion(new Rect(0, 0, ProjectManager.CurrentProject.Width, ProjectManager.CurrentProject.Height), ProjectManager.SelectedLayer);
            CurrentStrokeRegion = null;

            DrawPreview(imagePoint, ProjectManager.CurrentProject.Background);
            base.OnMouseUp(sender, e, imagePoint);
        }

        public override void DrawPreview(Point p, Color color)
        {
            var diagonal = Math.Sqrt(2) * BrushSize / 2;
            int x1 = (int)(p.X - diagonal);
            int y1 = (int)(p.Y - diagonal);
            int x2 = (int)(p.X + diagonal);
            int y2 = (int)(p.Y + diagonal);
            var region = new Rect(
                x1 - PREVIEW_STROKE_REGION_PADDING,
                y1 - PREVIEW_STROKE_REGION_PADDING,
                x2 - x1 + PREVIEW_STROKE_REGION_PADDING * 2,
                y2 - y1 + PREVIEW_STROKE_REGION_PADDING * 2);

            if (IsValidDrawingState() && CurrentStrokeRegion == null)
            {
                ProjectManager.StrokeLayer.FillRectangle(
                x1 - PREVIEW_STROKE_REGION_PADDING,
                y1 - PREVIEW_STROKE_REGION_PADDING,
                x2 + PREVIEW_STROKE_REGION_PADDING,
                y2 + PREVIEW_STROKE_REGION_PADDING,
                Colors.Transparent);
                ProjectManager.StrokeLayer.FillRectangle(x1, y1, x2, y2, color);
                ProjectManager.StrokeLayer.DrawRectangle(x1 - 1, y1 - 1, x2 + 1, y2 + 1, Colors.Black);
                ProjectManager.InvalidateRegion(region, ProjectManager.SelectedLayer);
            }
        }

        public override void DrawPoint(Point point, Color color)
        {
            //try
            //{
            //    var totalPadding = BrushSize + STROKE_REGION_PADDING;
            //    var region = new Rect(
            //        point.X - totalPadding,
            //        point.Y - totalPadding,
            //        totalPadding * 2,
            //        totalPadding * 2
            //    );

            //    var diagonal = Math.Sqrt(2) * BrushSize / 2;
            //    ProjectManager.StrokeLayer.FillRectangle(
            //        (int)(point.X - diagonal), (int)(point.Y - diagonal), (int)(point.X + diagonal), (int)(point.Y + diagonal), color);

            //    if (CurrentStrokeRegion.HasValue)
            //    {
            //        CurrentStrokeRegion = Rect.Union(CurrentStrokeRegion.Value, region);
            //    }

            //    ProjectManager.InvalidateRegion(region, ProjectManager.SelectedLayer);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"Error drawing point: {ex.Message}");
            //}
        }

        public override void DrawLine(Point start, Point end, Color color)
        {
            //try
            //{
            //    float distance = CalculateDistance(start, end);
            //    int steps = Math.Max(1, (int)(distance / (BrushSize * INTERP_FACTOR)));

            //    Point lastDrawnPoint = start;

            //    for (int i = 0; i <= steps; i++)
            //    {
            //        float t = i / (float)steps;
            //        var currentPoint = Lerp(start, end, t);

            //        var region = CalculateSegmentRegion(lastDrawnPoint, currentPoint, BrushSize);

            //        var diagonal = Math.Sqrt(2) * BrushSize / 2;
            //        ProjectManager.StrokeLayer.FillRectangle(
            //        (int)(currentPoint.X - diagonal), (int)(currentPoint.Y - diagonal), (int)(currentPoint.X + diagonal), (int)(currentPoint.Y + diagonal), color);
            //        if (CurrentStrokeRegion.HasValue)
            //        {
            //            CurrentStrokeRegion = Rect.Union(CurrentStrokeRegion.Value, region);
            //        }

            //        ProjectManager.InvalidateRegion(region, ProjectManager.SelectedLayer);
            //        lastDrawnPoint = currentPoint;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"Error drawing line: {ex.Message}");
            //}
        }
    }
}