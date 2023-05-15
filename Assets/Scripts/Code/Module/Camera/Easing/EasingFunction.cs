/*
* Copyright (c) 2017-2018 Alan Yeats http://alanyeats.com
*
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaoTie
{
    /// <summary>
    /// Easing Lerp Implmentation to create effective lerp functions 
    /// 
    /// Note: See http://easings.net/ for demo of all functions 
    /// </summary>
    public class EasingFunction
    {
        /// <summary>
        /// The in and out of lerp function 
        /// </summary>
        public enum EasingInOutType
        {
            EaseIn,
            EaseOut,
            EaseInOut
        }

        /// <summary>
        /// The type of ease function
        /// </summary>
        public enum EasingLerpsType
        {
            Sine,
            Quad,
            Cubic,
            Quint,
            Expo,
            Circ,
            Back,
            Elastic,
            Bounce
        }

        /// <summary>
        /// Easy use of all Easing lerp function   
        /// </summary>
        /// <param name="type">The type of Lerp</param>
        /// <param name="inOutType">Which type of effect</param>
        /// <param name="time">Current time</param>
        /// <param name="a">Starting Value</param>
        /// <param name="b">Ending Value</param>
        /// <param name="lerpTotal">Lerp total time</param>
        /// <returns>Lerp Value</returns>
        public static float EasingLerp(EasingLerpsType type, EasingInOutType inOutType, float time, float a, float b,
            float lerpTotal = 1.0f)
        {
            switch (type)
            {
                case EasingLerpsType.Sine:
                {
                    return EaseSine(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Quad:
                {
                    return EaseQuad(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Cubic:
                {
                    return EaseCubic(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Quint:
                {
                    return EaseQuint(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Expo:
                {
                    return EaseExpo(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Circ:
                {
                    return EaseCirc(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Back:
                {
                    return EaseBack(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Elastic:
                {
                    return EaseElastic(inOutType, time, a, b, lerpTotal);
                }
                case EasingLerpsType.Bounce:
                {
                    return EaseBounce(inOutType, time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseSine(inOutType, time, a, b, lerpTotal);
                }
            }

        }

        //////////////////////////////////////////////
        // Sine Function
        /////////////////////////////////////////////

        #region Sine

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseSine(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInSine(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutSine(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutSine(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInSine(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        /// Ease In Sine
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInSine(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return -b * (float) Math.Cos(time / lerpTotal * (Math.PI / 2)) + b + a;
        }


        /// <summary>
        /// Ease out Sine
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutSine(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * (float) Math.Sin(time / lerpTotal * (Math.PI / 2)) + a;
        }



        /// <summary>
        /// Ease int out Sine
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutSine(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return -b / 2 * ((float) Math.Cos(Math.PI * time / lerpTotal) - 1) + a;
        }

        #endregion

        //////////////////////////////////////////////
        // Quad Function
        /////////////////////////////////////////////

        #region Quad


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseQuad(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInQuad(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutQuad(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutQuad(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInQuad(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInQuad(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * (time /= lerpTotal) * time + a;
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutQuad(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return -b * (time /= lerpTotal) * (time - 2) + a;
        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutQuad(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if ((time /= lerpTotal / 2) < 1)
            {
                return b / 2 * time * time + a;
            }

            return -b / 2 * ((--time) * (time - 2) - 1) + a;
        }

        #endregion


        //////////////////////////////////////////////
        // Cubic Function
        /////////////////////////////////////////////

        #region Cubic

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseCubic(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInCubic(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutCubic(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutCubic(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInCubic(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInCubic(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * (time /= lerpTotal) * time * time + a;
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutCubic(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * ((time = time / lerpTotal - 1) * time * time + 1) + a;
        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutCubic(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if ((time /= lerpTotal / 2) < 1)
            {
                return b / 2 * time * time * time + a;
            }

            return b / 2 * ((time -= 2) * time * time + 2) + a;
        }

        #endregion

        //////////////////////////////////////////////
        // Quart  Function
        /////////////////////////////////////////////

        #region Quart

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseQuart(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInQuart(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutQuart(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutQuart(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInQuart(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInQuart(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * (time /= lerpTotal) * time * time * time + a;
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutQuart(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return -b * ((time = time / lerpTotal - 1) * time * time * time - 1) + a;
        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutQuart(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if ((time /= lerpTotal / 2) < 1)
            {
                return b / 2 * time * time * time * time + a;
            }

            return -b / 2 * ((time -= 2) * time * time * time - 2) + a;
        }

        #endregion

        //////////////////////////////////////////////
        // Quint  Function
        /////////////////////////////////////////////

        #region Quint

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseQuint(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInQuint(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutQuint(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutQuint(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInQuint(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInQuint(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * (time /= lerpTotal) * time * time * time * time + a;
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutQuint(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * ((time = time / lerpTotal - 1) * time * time * time * time + 1) + a;
        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutQuint(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if ((time /= lerpTotal / 2) < 1)
            {
                return b / 2 * time * time * time * time * time + a;
            }

            return b / 2 * ((time -= 2) * time * time * time * time + 2) + a;
        }

        #endregion

        //////////////////////////////////////////////
        // Expo   Function
        /////////////////////////////////////////////

        #region Expo

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseExpo(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInExpo(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutExpo(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutExpo(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInExpo(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInExpo(float time, float a, float b, float lerpTotal = 1.0f)
        {
            if (time == 0)
            {
                return a;
            }
            else
            {
                return b * (float) Math.Pow(2, 10 * (time / lerpTotal - 1)) + a;
            }
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutExpo(float time, float a, float b, float lerpTotal = 1.0f)
        {


            if (time == lerpTotal)
            {
                return a + b;
            }
            else
            {
                return b * (-(float) Math.Pow(2, -10 * time / lerpTotal) + 1) + a;
            }

        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutExpo(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if (time == 0)
            {
                return a;
            }
            else if (time == lerpTotal)
            {
                return a + b;
            }

            if ((time /= lerpTotal / 2) < 1)
            {
                return b / 2 * (float) Math.Pow(2, 10 * (time - 1)) + a;
            }

            return b / 2 * (-(float) Math.Pow(2, -10 * --time) + 2) + a;
        }

        #endregion


        //////////////////////////////////////////////
        // Circ Function
        /////////////////////////////////////////////

        #region Circ

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseCirc(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInCirc(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutCirc(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutCirc(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInCirc(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        /// Ease In Sine
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInCirc(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return -b * ((float) Math.Sqrt(1 - (time /= lerpTotal) * time) - 1) + a;
        }


        /// <summary>
        /// Ease out Sine
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutCirc(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b * (float) Math.Sqrt(1 - (time = time / lerpTotal - 1) * time) + a;
        }



        /// <summary>
        /// Ease int out Sine
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutCirc(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if ((time /= lerpTotal / 2) < 1)
            {
                return -b / 2 * ((float) Math.Sqrt(1 - time * time) - 1) + a;
            }

            return b / 2 * ((float) Math.Sqrt(1 - (time -= 2) * time) + 1) + a;
        }

        #endregion

        //////////////////////////////////////////////
        // Back  Function
        /////////////////////////////////////////////

        #region Back

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseBack(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInBack(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutBack(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutBack(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInBack(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="s">overshoot value</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInBack(float time, float a, float b, float lerpTotal = 1.0f, float s = 1.70158f)
        {
            return b * (time /= lerpTotal) * time * ((s + 1) * time - s) + a;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="s">overshoot value</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutBack(float time, float a, float b, float lerpTotal = 1.0f, float s = 1.70158f)
        {

            return b * ((time = time / lerpTotal - 1) * time * ((s + 1) * time + s) + 1) + a;
        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="s">overshoot value</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutBack(float time, float a, float b, float lerpTotal = 1.0f, float s = 1.70158f)
        {

            if ((time /= lerpTotal / 2) < 1)
            {
                return b / 2 * (time * time * (((s *= (1.525f)) + 1) * time - s)) + a;
            }

            return b / 2 * ((time -= 2) * time * (((s *= (1.525f)) + 1) * time + s) + 2) + a;
        }

        #endregion


        //////////////////////////////////////////////
        // Elastic  Function
        /////////////////////////////////////////////

        #region Elastic

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseElastic(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInElastic(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutElastic(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutElastic(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInElastic(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInElastic(float time, float a, float b, float lerpTotal = 1.0f)
        {
            //if (t == 0) return b; if ((t /= d) == 1) return b + c;
            //float p = d * .3f;
            //float a = c;
            //float s = p / 4;
            //float postFix = a * pow(2, 10 * (t -= 1)); // this is a fix, again, with post-increment operators
            //return -(postFix * sin((t * d - s) * (2 * PI) / p)) + b;

            if (time == 0)
            {
                return a;
            }

            if ((time /= lerpTotal) == 1)
            {
                return a + b;
            }

            float p = lerpTotal * .3f;
            float i = b;
            float s = p / 4;
            float postFix = i * (float) Math.Pow(2, 10 * (time -= 1));

            return -(postFix * (float) Math.Sin((time * lerpTotal - s) * (2 * Math.PI) / p)) + a;
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutElastic(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if (time == 0)
            {
                return a;
            }

            if ((time /= lerpTotal) == 1)
            {
                return a + b;
            }

            float p = lerpTotal * .3f;
            float i = b;
            float s = p / 4;
            return (i * (float) Math.Pow(2, -10 * time) *
                (float) Math.Sin((time * lerpTotal - s) * (2 * (float) Math.PI) / p) + b + a);
        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutElastic(float time, float a, float b, float lerpTotal = 1.0f)
        {
            if (time == 0)
            {
                return a;
            }

            if ((time /= lerpTotal / 2) == 2)
            {
                return a + b;

            }

            float p = lerpTotal * (.3f * 1.5f);
            float i = b;
            float s = p / 4;

            if (time < 1)
            {
                return -.5f * (i * (float) Math.Pow(2, 10 * (time -= 1)) *
                               (float) Math.Sin((time * lerpTotal - s) * (2 * (float) Math.PI) / p)) + a;
            }

            return i * (float) Math.Pow(2, -10 * (time -= 1)) *
                (float) Math.Sin((time * lerpTotal - s) * (2 * (float) Math.PI) / p) * .5f + b + a;
        }

        #endregion

        //////////////////////////////////////////////
        // Bounce   Function
        /////////////////////////////////////////////

        #region Bounce

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inOutType">How should use sine lerp </param>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseBounce(EasingInOutType inOutType, float time, float a, float b, float lerpTotal = 1.0f)
        {
            switch (inOutType)
            {
                case EasingInOutType.EaseIn:
                {
                    return EaseInBounce(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseOut:
                {
                    return EaseOutBounce(time, a, b, lerpTotal);
                }
                case EasingInOutType.EaseInOut:
                {
                    return EaseInOutBounce(time, a, b, lerpTotal);
                }
                default:
                {
                    return EaseInBounce(time, a, b, lerpTotal);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInBounce(float time, float a, float b, float lerpTotal = 1.0f)
        {

            return b - EaseOutBounce(lerpTotal - time, 0, b, lerpTotal) + a;
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseOutBounce(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if ((time /= lerpTotal) < (1 / 2.75f))
            {
                return b * (7.5625f * time * time) + a;
            }
            else if (time < (2 / 2.75f))
            {
                return b * (7.5625f * (time -= (1.5f / 2.75f)) * time + .75f) + a;
            }
            else if (time < (2.5 / 2.75))
            {
                return b * (7.5625f * (time -= (2.25f / 2.75f)) * time + .9375f) + a;
            }
            else
            {
                return b * (7.5625f * (time -= (2.625f / 2.75f)) * time + .984375f) + a;
            }
        }



        /// <summary>
        ///  
        /// </summary>
        /// <param name="time">Current Time of lerp</param>
        /// <param name="a">Beginning value of lerp</param>
        /// <param name="b">Final value of lerp</param>
        /// <param name="lerpTotal">Total time between lerps</param>
        /// <returns>Lerp Value</returns>
        static float EaseInOutBounce(float time, float a, float b, float lerpTotal = 1.0f)
        {

            if (time < lerpTotal / 2)
            {
                return EaseInBounce(time * 2, 0, b, lerpTotal) * .5f + a;
            }
            else
            {
                return EaseOutBounce(time * 2 - lerpTotal, 0, b, lerpTotal) * .5f + b * .5f + a;
            }
        }

        #endregion

    }

}