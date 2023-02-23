/*!
 * @file	ImageData.h
 * @author	kleon6436
 */

#pragma once

#include <iostream>
#include <vector>

/*!
 * @brief 画像データ
 */
typedef struct ImageData
{
	std::vector<std::byte> buffer;
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
	bool isRawImage;
	bool isThumbnailMode;
	int resizeLongSideLength;
} ImageReadSettings;