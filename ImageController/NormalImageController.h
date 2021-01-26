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
		 * @param	uint8_t** buffer: byte buffer data (out)
		 * @param	unsigned int* size: buffer size (out)
		 * @param   int* stride: Stride data (out)
		 * @param   int* width: Image width (out)
		 * @param   int* height: Image height (out)
		 *
		 * @return	Success: 0, Failure: 1
		 */
		int GetImageData(const char* path, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) const override;

		/**
		 * @brief	This function is getting thumbnail image data.
		 * 
		 * @param	const char* path: Raw image file path.
		 * @param	int resizeLongSideLength: Long side length of a resize image.
		 * @param	uint8_t** buffer: byte buffer data (out)
		 * @param	unsigned int* size: buffer size (out)
		 * @param   int* stride: Stride data (out)
		 * @param   int* width: Image width (out)
		 * @param   int* height: Image height (out)
		 * 
		 * @return	Success: 0, Failure: 1
		 */
		int GetThumbnailImageData(const char* path, int resizeLongSideLength, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) const override;

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