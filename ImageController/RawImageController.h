/**
 * @file	RawImageController.h
 * @author	kchary6436
 */

#ifndef RAWIMAGECONTROLLER_H_
#define RAWIMAGECONTROLLER_H_

#include <opencv2/opencv.hpp>
#include <libraw/libraw_types.h>
#include "IImageController.h"

namespace Kchary::ImageController::RawImageControl
{
	class RawImageController : public IImageController
	{
	public:
		/**
		 * @brief	This function is getting image data using libraw library.
		 *			Get from a thumbnail image that can be read at high speed for display.
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
		static cv::ImreadModes GetImreadMode(libraw_thumbnail_t thumbnail, int resizeLongSideLength);
	};
}

#endif // RAWIMAGECONTROLLER_H_