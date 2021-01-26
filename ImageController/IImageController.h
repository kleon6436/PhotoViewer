/**
 * @file	IImageController.h
 * @author	kchary6436
 */

#ifndef IIMAGECONTROLLER_H_
#define IIMAGECONTROLLER_H_

#include <stdint.h>

class IImageController
{
public:
	virtual ~IImageController() {}

	/**
	 * @brief	This function is getting image data.
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
	virtual int GetImageData(const char* path, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) = 0;

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
	virtual int GetThumbnailImageData(const char* path, int resizeLongSideLength, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) = 0;
};

#endif // IIMAGECONTROLLER_H_