/**
 * @file	NormalImageController.h
 * @author	kchary6436
 */

#ifndef NORMALIMAGECONTROLLER_H_
#define NORMALIMAGECONTROLLER_H_

#include <iostream>
#include <opencv2/opencv.hpp>
#include "IImageController.h"

namespace Kchary::ImageController::NormalImageControl
{
	class NormalImageController : public IImageController
	{
	public:
		/**
		 * @brief	This function is getting image data using libraw library.
		 * 
		 * @param	const char* path: Raw image file path.
		 * @param	ImageData* imageData: Image data
		 *
		 * @return	Success: 0, Failure: -1
		 */
		int GetImageData(const char* path, ImageData* imageData) const override;

		/**
		 * @brief	This function is getting thumbnail image data.
		 * 
		 * @param	const char* path: Raw image file path.
		 * @param	int resizeLongSideLength: Long side length of a resize image.
		 * @param	ImageData* imageData: Image data
		 * 
		 * @return	Success: 0, Failure: -1
		 */
		int GetThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData) const override;

	private:
		/**
		 * @brief	This function is getting thumbnail image data.
		 *
		 * @param	libraw_thumbnail_t: Thumbnail image data structure.
		 * @param	int resizeLongSideLength: Long side length of a resize image.
		 *
		 * @return	ImreadModes
		 */
		cv::ImreadModes GetImreadMode(int resizeLongSideLength) const;
	};
}

#endif // NORMALIMAGECONTROLLER_H_