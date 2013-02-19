using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;


namespace Sanguosha.UI.Effects
{
    /// <summary>An effect that blends between partial desaturation and a two-color ramp.</summary>
    public class RippleTransitionEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(RippleTransitionEffect), 0);
        public static readonly DependencyProperty Texture2Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Texture2", typeof(RippleTransitionEffect), 1);
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(RippleTransitionEffect), new UIPropertyMetadata(((double)(30D)), PixelShaderConstantCallback(0)));
        public RippleTransitionEffect()
        {
            var pixelShader = new PixelShader();
            pixelShader.UriSource = Global.MakePackUri("ShaderSource/RippleTransition.ps");
            this.PixelShader = pixelShader;

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(Texture2Property);
            this.UpdateShaderValue(ProgressProperty);
        }
        public Brush Input {
            get {
                return ((Brush)(this.GetValue(InputProperty)));
            }
            set {
                this.SetValue(InputProperty, value);
            }
        }
        public Brush Texture2 {
            get {
                return ((Brush)(this.GetValue(Texture2Property)));
            }
            set {
                this.SetValue(Texture2Property, value);
            }
        }
        /// <summary>The amount(%) of the transition from first texture to the second texture. </summary>
        public double Progress {
            get {
                return ((double)(this.GetValue(ProgressProperty)));
            }
            set {
                this.SetValue(ProgressProperty, value);
            }
        }
    }
}
