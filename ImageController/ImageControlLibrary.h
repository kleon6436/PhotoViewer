#pragma once

#ifdef __cplusplus
#define DllExport extern "C" __declspec(dllexport)
#else
#define DllExport __declspec(dllexport)
#endif

#include <iostream>

using namespace std;

namespace Kchary::ImageController::Library
{
	/**
	 * @brief	This function is getting raw image data using libraw library.
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
	DllExport int GetRawImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height);

	/**
	 * @brief	This function is getting raw thumbnail image data using libraw library.
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
	DllExport int GetRawThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height);

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
	DllExport int GetNormalImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height);

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
	DllExport int GetNormalThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height);

	/**
	 * @brief	Release the memory acquired on the DLL side.
	 *
	 * @param	uint8_t* buffer: Memory pointer you want to release.
	 */
	DllExport void FreeBuffer(uint8_t* buffer);
}