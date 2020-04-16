using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiaTest.Test
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TestPage : ContentPage
	{
		Random rnd = new Random();
		Queue<SKRect> lines = new Queue<SKRect>();
		SKRect lastLine;
		float dStartX, dStartY, dEndX, dEndY;
		SKPaint paintGPU, paintCPU;
		bool running = false;

		public TestPage()
		{
			InitializeComponent();

			if (Device.Idiom == TargetIdiom.Phone)
				ViewsLayout.Orientation = StackOrientation.Vertical;

			paintGPU = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				Color = SKColors.Red,
				StrokeWidth = 1,
				IsAntialias = true,
			};

			paintCPU = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				Color = SKColors.Green,
				StrokeWidth = 1,
				IsAntialias = true,
			};

			lastLine = new SKRect(rnd.Next(300), rnd.Next(300), rnd.Next(300), rnd.Next(300));

			dStartX = rnd.Next(5, 10);
			dStartY = rnd.Next(5, 10);
			dEndX = rnd.Next(5, 10);
			dEndY = rnd.Next(5, 10);

			Device.StartTimer(new TimeSpan(0,0,1), () =>
			{
				StartTimer();
				btnStart.IsEnabled = true;
				btnStop.IsEnabled = false;
				return false;
			});

			GPUView.PaintSurface += OnPaintGPUSurface;
			CPUView.PaintSurface += OnPaintCPUSurface;
		}

		private void OnBtnStartClicked(object sender, EventArgs args)
		{
			running = true;
			btnStart.IsEnabled = false;
			btnStop.IsEnabled = true;
		}

		private void OnBtnStopClicked(object sender, EventArgs args)
		{
			running = false;
			btnStart.IsEnabled = true;
			btnStop.IsEnabled = false;
		}

		private void StartTimer()
		{
			Device.StartTimer(new TimeSpan(50000), () =>
			{
				if (!running || GPUView.CanvasSize.Width < 0)
					return true;

				if (lines.Count > 40)
					lines.Dequeue();

				// Add new line
				lastLine = GetNextLine();
				lines.Enqueue(lastLine);

				CPUView.InvalidateSurface();
				GPUView.InvalidateSurface();

				return true;
			});
		}

		private void OnPaintCPUSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;

			OnPaintSurface(canvas, paintCPU);
		}

		private void OnPaintGPUSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;

			OnPaintSurface(canvas, paintGPU);
		}

		private void OnPaintSurface(SKCanvas canvas, SKPaint paint)
		{
			canvas.Clear();

			foreach(var line in lines)
				canvas.DrawLine(line.Left, line.Top, line.Right, line.Bottom, paint);
		}

		private SKRect GetNextLine()
		{
			var nextStartX = lastLine.Left + dStartX;
			var nextStartY = lastLine.Top + dStartY;
			var nextEndX = lastLine.Right + dEndX;
			var nextEndY = lastLine.Bottom + dEndY;

			if (nextStartX < 0)
			{
				nextStartX = 0;
				dStartX = rnd.Next(5, 10);
				dStartY = -dStartY;
			}

			if (nextStartX > GPUView.CanvasSize.Width)
			{
				nextStartX = (float)GPUView.CanvasSize.Width;
				dStartX = -rnd.Next(5, 10);
				dStartY = -dStartY;
			}

			if (nextStartY < 0)
			{
				nextStartY = 0;
				dStartX = -dStartX;
				dStartY = rnd.Next(5, 10);
			}

			if (nextStartY > GPUView.CanvasSize.Height)
			{
				nextStartY = (float)GPUView.CanvasSize.Height;
				dStartX = -dStartX;
				dStartY = -rnd.Next(5, 10);
			}

			if (nextEndX < 0)
			{
				nextEndX = 0;
				dEndX = rnd.Next(5, 10);
				dEndY = -dEndY;
			}

			if (nextEndX > GPUView.CanvasSize.Width)
			{
				nextEndX = (float)GPUView.CanvasSize.Width;
				dEndX = -rnd.Next(5, 10);
				dEndY = -dEndY;
			}

			if (nextEndY < 0)
			{
				nextEndY = 0;
				dEndX = -dEndX;
				dEndY = rnd.Next(5, 10);
			}

			if (nextEndY > GPUView.CanvasSize.Height)
			{
				nextEndY = (float)GPUView.CanvasSize.Height;
				dEndX = -dEndX;
				dEndY = -rnd.Next(5, 10);
			}

			return new SKRect(nextStartX, nextStartY, nextEndX, nextEndY);
		}
	}
}