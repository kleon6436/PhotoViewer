/**
 * @file	IImageController.h
 * @author	kleon6436
 */

#pragma once

#include "ImageData.h"

class IImageController
{
public:
	virtual ~IImageController() = default;

	/**
	 * @brief	This function is getting image data.
	 * @param	path: Raw image file path.
	 * @param	imageData: Image data
	 * @return	Success: 0, Failure: -1
	 */
	virtual int GetImageData(const char* path, ImageData* imageData) const = 0;

	/**
	 * @brief	This function is getting thumbnail image data.
	 * @param	path: Raw image file path.
	 * @param	resizeLongSideLength: Long side length of a resize image.
	 * @param	imageData: Image data
	 * @return	Success: 0, Failure: -1
	 */
	virtual int GetThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData) const = 0;
};