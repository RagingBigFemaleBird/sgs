using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;


namespace Sanguosha.UI.Effects
{
    /// <summary>An effect that intensifies bright areas.</summary>
	public class LightStreakEffect : ShaderEffect {
		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(LightStreakEffect), 0);
		public static readonly DependencyProperty BrightThresholdProperty = DependencyProperty.Register("BrightThreshold", typeof(double), typeof(LightStreakEffect), new UIPropertyMetadata(((double)(0.5D)), PixelShaderConstantCallback(0)));
		public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(LightStreakEffect), new UIPropertyMetadata(((double)(0.5D)), PixelShaderConstantCallback(1)));
		public static readonly DependencyProperty AttenuationProperty = DependencyProperty.Register("Attenuation", typeof(double), typeof(LightStreakEffect), new UIPropertyMetadata(((double)(0.25D)), PixelShaderConstantCallback(2)));
		public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(Vector), typeof(LightStreakEffect), new UIPropertyMetadata(new Vector(0.5D, 1D), PixelShaderConstantCallback(3)));
		public static readonly DependencyProperty InputSizeProperty = DependencyProperty.Register("InputSize", typeof(Size), typeof(LightStreakEffect), new UIPropertyMetadata(new Size(800D, 600D), PixelShaderConstantCallback(4)));
		
        static PixelShader pixelShader;

        static LightStreakEffect()
        {
            pixelShader = new PixelShader();
            pixelShader.UriSource = Global.MakePackUri("ShaderSource/LightStreak.ps"); 
        }

        public LightStreakEffect() {		
			this.PixelShader = pixelShader;

			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(BrightThresholdProperty);
			this.UpdateShaderValue(ScaleProperty);
			this.UpdateShaderValue(AttenuationProperty);
			this.UpdateShaderValue(DirectionProperty);
			this.UpdateShaderValue(InputSizeProperty);
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		/// <summary>Threshold for selecting bright pixels.</summary>
		public double BrightThreshold {
			get {
				return ((double)(this.GetValue(BrightThresholdProperty)));
			}
			set {
				this.SetValue(BrightThresholdProperty, value);
			}
		}
		/// <summary>Contrast factor.</summary>
		public double Scale {
			get {
				return ((double)(this.GetValue(ScaleProperty)));
			}
			set {
				this.SetValue(ScaleProperty, value);
			}
		}
		/// <summary>Attenuation factor.</summary>
		public double Attenuation {
			get {
				return ((double)(this.GetValue(AttenuationProperty)));
			}
			set {
				this.SetValue(AttenuationProperty, value);
			}
		}
		/// <summary>Direction of light streaks (as a vector).</summary>
		public Vector Direction {
			get {
				return ((Vector)(this.GetValue(DirectionProperty)));
			}
			set {
				this.SetValue(DirectionProperty, value);
			}
		}
		/// <summary>Size of the input (in pixels).</summary>
		public Size InputSize {
			get {
				return ((Size)(this.GetValue(InputSizeProperty)));
			}
			set {
				this.SetValue(InputSizeProperty, value);
			}
		}
	}
}

 