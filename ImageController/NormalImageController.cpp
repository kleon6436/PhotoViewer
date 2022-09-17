#include "pch.h"
/**
 * @file	NormalImageController.cpp
 * @author	kleon6436
 */

#include "NormalImageController.h"

namespace Kchary::ImageController::NormalImageControl
{
	bool NormalImageController::LoadImageAndGetImageSize(const char* path, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		const auto pathStr = std::string(path);

		if (imageReadSettings.isThumbnailMode)
		{
			// Read image data to mat.
			auto image = cv::imread(path, GetImreadMode(imageReadSettings.resizeLongSideLength));

			// Resize image data.
			const auto longSideLength = image.cols > image.rows ? image.cols : image.rows;
			const auto ratio = (static_cast<double>(imageReadSettings.resizeLongSideLength) / static_cast<double>(longSideLength));
			cv::Mat resizeImg;
			cv::resize(image, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

			m_image = resizeImg;
			imageSize = static_cast<int>(resizeImg.total() * resizeImg.elemSize() * sizeof(std::uint8_t));
		}
		else
		{
			// Read image data to mat.
			m_image = cv::imread(path, cv::ImreadModes::IMREAD_COLOR);
			imageSize = static_cast<int>(m_image.total() * m_image.elemSize() * sizeof(std::uint8_t));
		}

		return true;
	}

	bool NormalImageController::GetImageData(ImageData& imageData)
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

	bool NormalImageController::GetThumbnailImageData(ImageData& imageData)
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

	cv::ImreadModes NormalImageController::GetImreadMode(const int resizeLongSideLength)
	{
		cv::ImreadModes imreadMode;

		if (resizeLongSideLength <= 800)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
		}
		else if (800 < resizeLongSideLength && resizeLongSideLength <= 1600)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
		}
		else if (1600 < resizeLongSideLength && resizeLongSideLength <= 3200)
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