﻿using System;
using System.Drawing;

namespace Steganography
{
    class SteganographyHelper
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)//이미지에 텍스트를 숨기기 위한 메소드입니다.
        {
            State state = State.Hiding;//state state는 state.hiding로 하고

            int charIndex = 0;//숨길 텍스트를 문자 단위로 인덱스 표현

            int charValue = 0;//문자 단위로 값 초기화

            long pixelElementIndex = 0;// 이미지 픽셀 R,G,B에 char단위로 저장하는 변수 설정 

            int zeros = 0;//마지막 데이터를 표현할때 문자를 카운터하는 변수 설정

            int R = 0, G = 0, B = 0; //R,G,B값 0으로 초기화

            for (int i = 0; i < bmp.Height; i++)//배열 형태의 이미지
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);//픽셀 값을 i,l에 저장

                    R = pixel.R - pixel.R % 2;//픽셀 R의 LSB를 2로 나누어 0으로 저장
                    G = pixel.G - pixel.G % 2;//픽셀 G의 LSB를 2로 나누어 0으로 저장
                    B = pixel.B - pixel.B % 2;//픽셀 B의 LSB를 2로 나누어 0으로 저장

                    for (int n = 0; n < 3; n++)//픽셀에 해당되는 값을 세팅하기 위한 반복
                    {
                        if (pixelElementIndex % 8 == 0)//0이거나 8의 배수(다음 수행할 문자 지정)
                        {
                            if (state == State.Filling_With_Zeros && zeros == 8)//데이터를 슴기고 8의 배수도 다 수행이 된 경우
                            {
                                if ((pixelElementIndex - 1) % 3 < 2)//픽셀에 B값이 저장된 경우
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//해당 픽셀에 나머지 R,G값 세팅
                                }

                                return bmp;// 적용된 BMP을 반환
                            }

                            if (charIndex >= text.Length)// 해당 텍스트를 전부 다 저장한 경우
                            {
                                state = State.Filling_With_Zeros;//state는 filling with zeoros를 가리킴.
                            }
                            else 
                            {
                                charValue = text[charIndex++];//해당 텍스트에 아직 남아있는 경우엔 char인덱스를 1증가하고 다시 저장
                            }
                        }

                        switch (pixelElementIndex % 3)//R,G,B값을 다시 수행해서 해당 문자를 저장하기 위해 수행 
                        {
                            case 0://case가 1인경우(픽셀에 R인경우)
                                {
                                    if (state == State.Hiding)//state가 state.hiding를 가리키면
                                    {
                                        R += charValue % 2;//LSB에 R을 저장
                                        charValue /= 2;//해당 값을 2로 나눔
                                    }
                                } break;
                            case 1:// 픽셀에 G인 경우
                                {
                                    if (state == State.Hiding)//state가 state.hiding를 가리키면
                                    {
                                        G += charValue % 2;//LSB에 G를 저장

                                        charValue /= 2;//해당 값을 2로 나누어 이동
                                    }
                                } break;
                            case 2://픽셀에 B인 경우
                                {
                                    if (state == State.Hiding)//state가 state.hiding를 가리키면
                                    {
                                        B += charValue % 2;//LSB에 B를 저장

                                        charValue /= 2;//다시 2로 나누어 이동
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));//숨길 이미지 텍스트에 LSB에 RGB세팅
                                } break;
                        }

                        pixelElementIndex++;//1증가

                        if (state == State.Filling_With_Zeros)//만약 state가 filling with zeros일 경우
                        {
                            zeros++;//1증가
                        }
                    }
                }
            }

            return bmp;
        }

        public static string extractText(Bitmap bmp)//이미지의 비트를 추출하는 과정
        {
            int colorUnitIndex = 0;//하나하나 RGB에 인덱스 설정
            int charValue = 0;//문자의 값을 위한 변수

            string extractedText = String.Empty;//추출된 결과값을 저장하기 위해 설정

            for (int i = 0; i < bmp.Height; i++)//반복으로 각 방문한다
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);//픽셀 값을 j,i저장
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;//픽셀의 R에 LSB값추출한 후 결과값을 저장하는 곳에 저장
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;//픽셀의 G에 LSB값추출한 후 결과값을 저장하는 곳에 저장
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;//픽셀의 B에 LSB값추출한 후 결과값을 저장하는 곳에 저장
                                } break;
                        }

                        colorUnitIndex++;//값 1을 증가

                        if (colorUnitIndex % 8 == 0)//8회 수행
                        {
                            charValue = reverseBits(charValue);//값을 거꾸로 되있던 걸 다시 리벌스하는 것

                            if (charValue == 0)//만약 추출할 값이 0인 경우
                            {
                                return extractedText;//추출된텍스트를 반환
                            }
                            char c = (char)charValue;// 문자형으로 표현함

                            extractedText += c.ToString();//계속 문자형으로 저장함
                        }
                    }
                }
            }

            return extractedText;//추출된 텍스트 반환
        }

        public static int reverseBits(int n)//역 변환하기 위한 호출
        {
            int result = 0;//결과 값 0세팅

            for (int i = 0; i < 8; i++)// 8회 반복
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}