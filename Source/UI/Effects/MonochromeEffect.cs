using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.UI.Effects
{
    // (c) Copyright Microsoft Corporation.
    // This source is subject to the Microsoft Permissive License.
    // See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
    // All other rights reserved.


    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Diagnostics;
#if SILVERLIGHT 
using UIPropertyMetadata = System.Windows.PropertyMetadata ;      
#endif
    /// <summary>
    /// This is the implementation of an extensible framework ShaderEffect which loads
    /// a shader model 2 pixel shader. Dependecy properties declared in this class are mapped
    /// to registers as defined in the *.ps file being loaded below.
    /// </summary>
    public class MonochromeEffect : ShaderEffect
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets the FilterColor variable within the shader.
        /// </summary>
        public static readonly DependencyProperty FilterColorProperty = DependencyProperty.Register("FilterColor", typeof(Color), typeof(MonochromeEffect), new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Gets or sets the Strength variable within the shader.
        /// </summary>
        public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register("Strength", typeof(double), typeof(MonochromeEffect), new UIPropertyMetadata(1.0d, PixelShaderConstantCallback(1), CoerceStrength));

        /// <summary>
        /// Gets or sets the Input of the shader.
        /// </summary>
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MonochromeEffect), 0);

        #endregion
                
        #region Constructors
        
        /// <summary>
        /// Creates an instance and updates the shader's variables to the default values.
        /// </summary>
        public MonochromeEffect()
        {
            var pixelShader = new PixelShader();
            pixelShader.UriSource = Global.MakePackUri("ShaderSource/Monochrome.ps");
            this.PixelShader = pixelShader;            
            
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(FilterColorProperty);
            UpdateShaderValue(StrengthProperty);
        }

        #endregion

        /// <summary>
        /// Gets or sets the FilterColor variable within the shader.
        /// </summary>
        public Color FilterColor
        {
            get { return (Color)GetValue(FilterColorProperty); }
            set { SetValue(FilterColorProperty, value); }
        }

        public double Strength
        {
            get { return (double)GetValue(StrengthProperty); }
            set { SetValue(StrengthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the input used in the shader.
        /// </summary>
        [System.ComponentModel.BrowsableAttribute(false)]
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        private static object CoerceStrength(DependencyObject d, object value)
        {
            MonochromeEffect effect = (MonochromeEffect)d;
            double newStrength = (double)value;
            if (newStrength < 0.0 || newStrength > 1.0)
            {
                return effect.Strength;
            }
            return newStrength;
        }
    }
}
