/*!
 * @file	ImageData.h
 * @author	kleon6436
 */

#pragma once

#include <iostream>

/*!
 * @brief 画像データ
 */
typedef struct ImageData
{
	std::uint8_t* buffer;
	unsigned int size;
	int stride;
	int width;
	int height;
} ImageData;

/*!
* @brief 画像読み込み設定
*/
typedef struct ImageReadSettings
{
	int isRawImage;
	int isThumbnailMode;
	int resizeLongSideLength;
} ImageReadSettings;