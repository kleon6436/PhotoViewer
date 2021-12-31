/*!
 * @file	ImageData.h
 * @author	kleon6436
 */

#pragma once

/*!
 * @brief 画像データ構造体
 */
typedef struct ImageData
{
	std::uint8_t* buffer;
	unsigned int size;
	int stride;
	int width;
	int height;
} ImageData;