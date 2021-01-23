#pragma once

#include <stdint.h>

using namespace std;

class IImageController
{
public:
	virtual ~IImageController(){}

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
	virtual int GetImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) = 0;

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
	virtual int GetThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) = 0;
};