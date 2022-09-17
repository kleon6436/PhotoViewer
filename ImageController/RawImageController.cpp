#include "pch.h"
/**
 * @file	RawImageController.cpp
 * @author	kleon6436
 */

#include "RawImageController.h"
#include <memory>
#include <libraw/libraw.h>

namespace Kchary::ImageController::RawImageControl
{
	bool RawImageController::LoadImageAndGetImageSize(const char* path, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		const auto rawProcessor = std::make_unique<LibRaw>();

		if (imageReadSettings.isThumbnailMode)
		{
			// Read raw image using libraw.
			if (rawProcessor->open_file(path) != LIBRAW_SUCCESS)
			{
				rawProcessor->recycle();
				return false;
			}

			if (rawProcessor->unpack_thumb() != LIBRAW_SUCCESS)
			{
				rawProcessor->recycle();
				return false;
			}

			// Get thumbnail struct data.
			auto* thumbnail = rawProcessor->dcraw_make_mem_thumb();
			if (!thumbnail || thumbnail->type != LibRaw_image_formats::LIBRAW_IMAGE_JPEG)
			{
				rawProcessor->recycle();
				return false;
			}

			// Read thumbnail jpeg image data using opencv.
			// (Raw image's thumbnail data is jpeg.)
			const auto thumbnailDataSize = thumbnail->data_size;
			auto* const thumbnailData = static_cast<std::uint8_t*>(thumbnail->data);

			const std::vector<std::uint8_t> jpegData(thumbnailData, thumbnailData + thumbnailDataSize);
			auto img = cv::imdecode(jpegData, GetImreadMode(rawProcessor->imgdata.thumbnail, imageReadSettings.resizeLongSideLength));

			// Resize image data.
			const auto longSideLength = img.cols > img.rows ? img.cols : img.rows;
			const auto ratio = (static_cast<double>(imageReadSettings.resizeLongSideLength) / static_cast<double>(longSideLength));
			cv::Mat resizeImg;
			cv::resize(img, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

			m_image = resizeImg;
			imageSize = static_cast<int>(resizeImg.total() * resizeImg.elemSize() * sizeof(std::uint8_t));

			LibRaw::dcraw_clear_mem(thumbnail);
		}
		else
		{
			// Read raw image using libraw.
			if (rawProcessor->open_file(path) != LIBRAW_SUCCESS)
			{
				rawProcessor->recycle();
				return false;
			}

			// Unpack the image.
			if (rawProcessor->unpack() != LIBRAW_SUCCESS)
			{
				rawProcessor->recycle();
				return false;
			}

			if (rawProcessor->dcraw_process() != LIBRAW_SUCCESS)
			{
				rawProcessor->recycle();
				return false;
			}

			auto* const image = rawProcessor->dcraw_make_mem_image();
			if (!image || image->type != LIBRAW_IMAGE_BITMAP || image->colors != 3)
			{
				rawProcessor->recycle();
				return false;
			}

			const auto rawDataSize = image->data_size;
			auto* const rawData = static_cast<std::uint8_t*>(image->data);

			const std::vector<std::uint8_t> rawImage(rawData, rawData + rawDataSize);
			m_image = cv::imdecode(rawImage, cv::ImreadModes::IMREAD_COLOR);
			imageSize = static_cast<int>(image->data_size);

			LibRaw::dcraw_clear_mem(image);
		}

		rawProcessor->recycle();
		return true;
	}

	bool RawImageController::GetImageData(ImageData& imageData)
	{
		if (m_image.empty())
		{
			return false;
		}

		const auto dataSize = m_image.total() * m_image.elemSize();
		memcpy(imageData.buffer, m_image.data, dataSize * sizeof(std::uint8_t));

		imageData.size = static_cast<unsigned int>(dataSize);
		imageData.stride = static_cast<int>(m_image.step);
		imageData.width = m_image.cols;
		imageData.height = m_image.rows;
		return true;
	}

	bool RawImageController::GetThumbnailImageData(ImageData& imageData)
	{
		if (m_image.empty())
		{
			return false;
		}

		const auto dataSize = m_image.total() * m_image.elemSize();
		memcpy(imageData.buffer, m_image.data, dataSize * sizeof(std::uint8_t));

		// Translate data to C#
		imageData.size = static_cast<unsigned int>(dataSize);
		imageData.stride = static_cast<int>(m_image.step);
		imageData.width = m_image.cols;
		imageData.height = m_image.rows;

		return true;
	}

	cv::ImreadModes RawImageController::GetImreadMode(const libraw_thumbnail_t& thumbnail, const int resizeLongSideLength)
	{
		const auto thumbLongSideLength = thumbnail.twidth > thumbnail.theight ? thumbnail.twidth : thumbnail.theight;
		cv::ImreadModes imreadMode;

		if (resizeLongSideLength <= thumbLongSideLength / 8)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
		}
		else if (thumbLongSideLength / 8 < resizeLongSideLength && resizeLongSideLength <= thumbLongSideLength / 4)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
		}
		else if (thumbLongSideLength / 4 < resizeLongSideLength && resizeLongSideLength <= thumbLongSideLength / 2)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_2;
		}
		else
		{
			imreadMode = cv::ImreadModes::IMREAD_COLOR;
		}

		return imreadMode;
	}
}