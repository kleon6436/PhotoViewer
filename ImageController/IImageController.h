/**
 * @file	IImageController.h
 * @author	kchary6436
 */

#ifndef IIMAGECONTROLLER_H_
#define IIMAGECONTROLLER_H_

#include "ImageData.h"

class IImageController
{
public:
	virtual ~IImageController() {}

	/**
	 * @brief	This function is getting image data.
	 *
	 * @param	const char* path: Raw image file path.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	virtual int GetImageData(const char* path, ImageData* imageData) const = 0;

	/**
	 * @brief	This function is getting thumbnail image data.
	 *
	 * @param	const char* path: Raw image file path.
	 * @param	int resizeLongSideLength: Long side length of a resize image.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	virtual int GetThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData) const = 0;
};

#endif // IIMAGECONTROLLER_H_